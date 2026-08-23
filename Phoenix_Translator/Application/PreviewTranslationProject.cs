using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PhoenixEngine;
using PhoenixEngine.Memory;
using PhoenixEngine.Translate;
using PhoenixEngine.Unit;
using PhoenixTranslator.SkyrimManagement;
using static PexInterface.PexHeuristicAnalysis;

namespace PhoenixTranslator.ApplicationLayer
{
    /// <summary>
    /// Owns a loaded parser project and exposes normalized records to the preview workspace.
    /// </summary>
    internal sealed class PreviewTranslationProject : IPreviewTranslationProject
    {
        private const long MaximumProjectBytes = 512L * 1024L * 1024L;
        private const int MaximumEntryCount = 500000;
        private const int MaximumContextRelations = 200;
        private const int MaximumCodeCharacters = 2 * 1024 * 1024;
        private readonly ModFile _modFile;
        private readonly object _contextSync = new object();
        private bool _disposed;

        private PreviewTranslationProject(string path, ModFile modFile, IReadOnlyList<PreviewTranslationEntry> entries)
        {
            Path = path;
            DisplayName = System.IO.Path.GetFileName(path);
            _modFile = modFile;
            Entries = entries;
        }

        /// <summary>
        /// Gets the private absolute path used only at the file boundary.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the safe project display name.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the normalized project records.
        /// </summary>
        public IReadOnlyList<PreviewTranslationEntry> Entries { get; private set; }

        /// <summary>
        /// Opens and normalizes a supported translation project.
        /// </summary>
        /// <param name="path">The selected project path.</param>
        /// <returns>The loaded project.</returns>
        /// <exception cref="InvalidDataException">The path, size, format, or record count is unsupported.</exception>
        internal static PreviewTranslationProject Open(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new InvalidDataException("The selected project is unavailable.");
            }

            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > MaximumProjectBytes)
            {
                throw new InvalidDataException("The selected project exceeds the supported size limit.");
            }

            ModFile modFile = null;
            try
            {
                modFile = new ModFile(path);
                if (modFile.Type == GameFileType.Null)
                {
                    throw new InvalidDataException("The selected project format is unsupported.");
                }

                if (modFile.XmlReader != null)
                {
                    modFile.XmlReader.ThrowOnInvalidFormat = true;
                }

                modFile.Load();
                List<PreviewTranslationEntry> entries = CreateEntries(modFile);
                if (entries.Count > MaximumEntryCount)
                {
                    throw new InvalidDataException("The selected project contains too many translation records.");
                }

                return new PreviewTranslationProject(path, modFile, entries);
            }
            catch
            {
                modFile?.Close();
                throw;
            }
        }

        /// <summary>
        /// Translates one entry through the configured provider pipeline.
        /// </summary>
        /// <param name="entry">The entry to translate.</param>
        /// <param name="cancellationToken">Cancels provider work cooperatively.</param>
        /// <returns>The provider-produced target text.</returns>
        public string Translate(PreviewTranslationEntry entry, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (!Phoenix.CheckAvailableNodes())
            {
                throw new InvalidOperationException("No translation provider is enabled.");
            }

            _modFile.MakeReady();
            string emotion = string.Empty;
            if (_modFile.Type == GameFileType.ESP && _modFile.EspReader.Records.ContainsKey(entry.Key))
            {
                emotion = _modFile.EspReader.QueryEmotion(_modFile.EspReader.Records[entry.Key]);
            }

            var unit = new BaseUnit(
                _modFile.P_Translator.GetFileUniqueKey(),
                entry.Key,
                entry.Type,
                entry.SourceText,
                entry.TargetText,
                emotion,
                entry.Score);
            unit.Translated = string.Empty;
            cancellationToken.ThrowIfCancellationRequested();
            string result = _modFile.P_Translator.Translate(unit, cancellationToken, false).GetFrist().Translated;
            cancellationToken.ThrowIfCancellationRequested();
            return result ?? string.Empty;
        }

        /// <summary>
        /// Loads bounded parser-owned context for one stable normalized entry.
        /// </summary>
        /// <param name="entry">The selected project entry.</param>
        /// <param name="cancellationToken">Cancels relationship and asset work cooperatively.</param>
        /// <returns>The supported context snapshot, which may be empty.</returns>
        public PreviewEntryContext LoadContext(
            PreviewTranslationEntry entry,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            lock (_contextSync)
            {
                ThrowIfDisposed();
                cancellationToken.ThrowIfCancellationRequested();
                return LoadContextCore(entry, cancellationToken);
            }
        }

        private PreviewEntryContext LoadContextCore(
            PreviewTranslationEntry entry,
            CancellationToken cancellationToken)
        {
            var metadata = new List<PreviewContextMetadata>();
            var relations = new List<PreviewContextRelation>();
            var npcs = new List<PreviewNpcContext>();
            string code = string.Empty;
            string codeSource = string.Empty;

            AddCommonMetadata(entry, metadata);
            AddExactSourceRelations(entry, relations, cancellationToken);
            if (_modFile.Type == GameFileType.PEX)
            {
                LoadPexContext(entry, metadata, ref code, ref codeSource);
            }
            else if (_modFile.Type == GameFileType.ESP)
            {
                LoadEspContext(entry, metadata, relations, npcs, cancellationToken);
            }

            PreviewAssetContext asset = PreviewAssetContextLoader.Load(Path, entry.SourceText, cancellationToken);
            return new PreviewEntryContext(
                code,
                codeSource,
                metadata,
                relations.Take(MaximumContextRelations).ToList(),
                npcs,
                asset);
        }

        /// <summary>
        /// Persists all staged entry targets using the existing format writer and backup boundary.
        /// </summary>
        public void Save()
        {
            ThrowIfDisposed();
            foreach (PreviewTranslationEntry entry in Entries)
            {
                _modFile.Lex_Dictionary.UPDateTransText(entry.Key, entry.SourceText);
                if (entry.IsModified)
                {
                    _modFile.P_Translator.AutoSetLink(
                        entry.Key,
                        entry.SourceText,
                        new P_String(entry.TargetText, 1));
                }
            }

            _modFile.Save();
        }

        /// <summary>
        /// Writes staged targets to a validated new destination and leaves the source project unchanged.
        /// </summary>
        /// <param name="path">The destination path, which must not already exist.</param>
        /// <param name="cancellationToken">Cancels work before the destination becomes visible.</param>
        public void Export(string path, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("An export path is required.", nameof(path));
            }

            string destination = System.IO.Path.GetFullPath(path);
            string source = System.IO.Path.GetFullPath(Path);
            if (string.Equals(destination, source, StringComparison.OrdinalIgnoreCase) || File.Exists(destination))
            {
                throw new IOException("Export requires a new destination file.");
            }

            string sourceExtension = System.IO.Path.GetExtension(source);
            if (!string.Equals(
                System.IO.Path.GetExtension(destination),
                sourceExtension,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The export destination must preserve the project format.");
            }

            string directory = System.IO.Path.GetDirectoryName(destination);
            Directory.CreateDirectory(directory);
            string temporaryPath = System.IO.Path.Combine(
                directory,
                "." + System.IO.Path.GetFileNameWithoutExtension(destination) + "." +
                Guid.NewGuid().ToString("N") + sourceExtension);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                File.Copy(source, temporaryPath, false);
                using (PreviewTranslationProject exportedProject = Open(temporaryPath))
                {
                    Dictionary<string, PreviewTranslationEntry> stagedTargets = Entries
                        .GroupBy(entry => entry.Key, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
                    foreach (PreviewTranslationEntry exportedEntry in exportedProject.Entries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        PreviewTranslationEntry stagedEntry;
                        if (stagedTargets.TryGetValue(exportedEntry.Key, out stagedEntry))
                        {
                            exportedEntry.TargetText = stagedEntry.TargetText;
                        }
                    }

                    exportedProject.Save();
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (!File.Exists(temporaryPath) || new FileInfo(temporaryPath).Length == 0)
                {
                    throw new InvalidDataException("The exported project failed output validation.");
                }

                File.Move(temporaryPath, destination);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_contextSync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _modFile.Close();
            }
        }

        private static List<PreviewTranslationEntry> CreateEntries(ModFile modFile)
        {
            switch (modFile.Type)
            {
                case GameFileType.MCM:
                    return modFile.MCMReader.MCMItems.Select(item => CreateEntry(
                        modFile, item.Key, item.Type, item.EditorID, item.SourceText, item.TransText, 100)).ToList();
                case GameFileType.XML:
                    return modFile.XmlReader.XmlItems.Select(item => CreateEntry(
                        modFile, item.Key, item.Type, item.EditorID, item.SourceText, item.TransText, 100)).ToList();
                case GameFileType.JSON:
                    return modFile.RamCacheReader.RamLines.Select(item => CreateEntry(
                        modFile, item.Key, item.Type, item.Key, item.SourceText, item.TransText, item.Score)).ToList();
                case GameFileType.PEX:
                    return modFile.PexReader.Records.Values.Select(item => CreateEntry(
                        modFile,
                        item.UniqueKey,
                        "PEX",
                        item.StringTableID.ToString(),
                        item.Original,
                        string.Empty,
                        item.Score)).ToList();
                case GameFileType.ESP:
                    return modFile.EspReader.Records.Values.Select(item => CreateEntry(
                        modFile,
                        item.UniqueKey,
                        item.ParentSig,
                        item.FormID,
                        item.String,
                        string.Empty,
                        100)).ToList();
                default:
                    throw new InvalidDataException("The selected project format is unsupported.");
            }
        }

        private static void AddCommonMetadata(
            PreviewTranslationEntry entry,
            ICollection<PreviewContextMetadata> metadata)
        {
            metadata.Add(new PreviewContextMetadata("Record", entry.Record, "Phoenix Translator"));
            metadata.Add(new PreviewContextMetadata("Type", entry.Type, "Phoenix Translator"));
            metadata.Add(new PreviewContextMetadata("Stable key", entry.Key, "Phoenix Translator"));
            metadata.Add(new PreviewContextMetadata("Confidence", entry.Score.ToString("0.##"), "Phoenix Translator"));
        }

        private void AddExactSourceRelations(
            PreviewTranslationEntry selectedEntry,
            ICollection<PreviewContextRelation> relations,
            CancellationToken cancellationToken)
        {
            foreach (PreviewTranslationEntry entry in Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (relations.Count >= MaximumContextRelations)
                {
                    return;
                }

                if (!ReferenceEquals(entry, selectedEntry) &&
                    string.Equals(entry.SourceText, selectedEntry.SourceText, StringComparison.Ordinal))
                {
                    relations.Add(CreateRelation(entry, "Exact source match", "Phoenix Translator"));
                }
            }
        }

        private void LoadPexContext(
            PreviewTranslationEntry entry,
            ICollection<PreviewContextMetadata> metadata,
            ref string code,
            ref string codeSource)
        {
            PexStringItem item;
            if (!_modFile.PexReader.Records.TryGetValue(entry.Key, out item))
            {
                return;
            }

            metadata.Add(new PreviewContextMetadata(
                "String table ID",
                item.StringTableID.ToString(),
                "PexInterface"));
            if (item.FunctionRef != null)
            {
                metadata.Add(new PreviewContextMetadata(
                    "Function",
                    item.FunctionRef.FunctionName,
                    "PexInterface"));
            }

            string decompiledCode = _modFile.PexReader.PSCCode ?? string.Empty;
            code = decompiledCode.Length <= MaximumCodeCharacters
                ? decompiledCode
                : decompiledCode.Substring(0, MaximumCodeCharacters);
            codeSource = "PexInterface";
        }

        private void LoadEspContext(
            PreviewTranslationEntry entry,
            ICollection<PreviewContextMetadata> metadata,
            ICollection<PreviewContextRelation> relations,
            ICollection<PreviewNpcContext> npcs,
            CancellationToken cancellationToken)
        {
            RecordItem record;
            if (!_modFile.EspReader.Records.TryGetValue(entry.Key, out record))
            {
                return;
            }

            metadata.Add(new PreviewContextMetadata("Form ID", record.FormID, "EspReader"));
            metadata.Add(new PreviewContextMetadata("Editor ID", record.EditorID, "EspReader"));
            metadata.Add(new PreviewContextMetadata("Record signature", record.ParentSig, "EspReader"));
            metadata.Add(new PreviewContextMetadata("Field signature", record.ChildSig, "EspReader"));
            metadata.Add(new PreviewContextMetadata(
                "Occurrence",
                record.OccurrenceIndex.ToString(),
                "EspReader"));

            List<Character> characters;
            if (_modFile.EspReader.GameCharacters.TryGetValue(entry.Key, out characters))
            {
                foreach (Character character in characters.Take(50))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    npcs.Add(new PreviewNpcContext(
                        entry.Key,
                        character.Name,
                        character.Gender.ToString(),
                        character.VoiceType));
                }
            }

            if (record.ParentSig == "INFO" || record.ParentSig == "DIAL")
            {
                ManagedDialContext dialogue = _modFile.EspReader.GetDialContext(record);
                if (dialogue != null)
                {
                    var nodes = new List<ManagedDialNode>();
                    if (dialogue.Head != null)
                    {
                        nodes.Add(dialogue.Head);
                    }

                    nodes.AddRange(dialogue.Links ?? new List<ManagedDialNode>());
                    foreach (ManagedDialNode node in nodes)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        RecordItem related = _modFile.EspReader.GetRecordItemByOffsets(
                            false,
                            node.RecordOffset,
                            node.SubOffset);
                        AddEspRelation(relations, related, "Dialogue", node.EmotionType);
                    }
                }
            }
            else if (record.ParentSig == "BOOK")
            {
                EspReader.BookInFoItem book = _modFile.EspReader.GetBookInFo(record);
                if (book != null)
                {
                    AddEspRelation(
                        relations,
                        _modFile.EspReader.GetRecordItemByOffsets(false, book.RecordOffset, book.TittleSubOffset),
                        "Book title",
                        999);
                    AddEspRelation(
                        relations,
                        _modFile.EspReader.GetRecordItemByOffsets(false, book.RecordOffset, book.ContentSubOffset),
                        "Book content",
                        999);
                }
            }
        }

        private void AddEspRelation(
            ICollection<PreviewContextRelation> relations,
            RecordItem record,
            string relationship,
            uint emotionType)
        {
            if (record == null || relations.Count >= MaximumContextRelations ||
                relations.Any(relation => relation.EntryKey == record.UniqueKey &&
                    relation.Relationship == relationship))
            {
                return;
            }

            PreviewTranslationEntry entry = Entries.FirstOrDefault(candidate => candidate.Key == record.UniqueKey);
            string detail = emotionType == 999
                ? record.ParentSig + " / " + record.ChildSig
                : EmotionTypeHelper.FromRaw(emotionType) + " · " + record.ParentSig + " / " + record.ChildSig;
            relations.Add(new PreviewContextRelation(
                record.UniqueKey,
                entry == null ? record.String : entry.SourceText,
                detail,
                "EspReader",
                relationship));
        }

        private static PreviewContextRelation CreateRelation(
            PreviewTranslationEntry entry,
            string relationship,
            string source)
        {
            return new PreviewContextRelation(
                entry.Key,
                entry.SourceText,
                entry.Record,
                source,
                relationship);
        }

        private static PreviewTranslationEntry CreateEntry(
            ModFile modFile,
            string key,
            string type,
            string record,
            string sourceText,
            string fallbackTarget,
            double score)
        {
            P_String linkedValue = modFile.P_Translator.GetLink()[key];
            string targetText = linkedValue == null ? fallbackTarget : linkedValue.String;
            return new PreviewTranslationEntry(key, type, record, sourceText, targetText, score);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PreviewTranslationProject));
            }
        }
    }
}
