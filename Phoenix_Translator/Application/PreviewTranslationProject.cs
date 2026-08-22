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

namespace PhoenixTranslator.ApplicationLayer
{
    /// <summary>
    /// Owns a loaded parser project and exposes normalized records to the preview workspace.
    /// </summary>
    internal sealed class PreviewTranslationProject : IPreviewTranslationProject
    {
        private const long MaximumProjectBytes = 512L * 1024L * 1024L;
        private const int MaximumEntryCount = 500000;
        private readonly ModFile _modFile;
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _modFile.Close();
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
