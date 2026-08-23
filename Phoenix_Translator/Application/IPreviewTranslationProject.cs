using System;
using System.Collections.Generic;
using System.Threading;

namespace PhoenixTranslator.ApplicationLayer
{
    /// <summary>
    /// Defines the parser, provider, and persistence boundary consumed by the preview workspace.
    /// </summary>
    internal interface IPreviewTranslationProject : IDisposable
    {
        /// <summary>
        /// Gets the private absolute path used only at parser and persistence boundaries.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the safe project display name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the normalized editable project records.
        /// </summary>
        IReadOnlyList<PreviewTranslationEntry> Entries { get; }

        /// <summary>
        /// Translates one entry through the configured provider pipeline.
        /// </summary>
        /// <param name="entry">The entry to translate.</param>
        /// <param name="cancellationToken">Cancels provider work cooperatively.</param>
        /// <returns>The provider-produced target text.</returns>
        string Translate(PreviewTranslationEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Loads bounded code, record, NPC, relationship, and asset context for one stable entry.
        /// </summary>
        /// <param name="entry">The selected normalized entry.</param>
        /// <param name="cancellationToken">Cancels parser and asset work cooperatively.</param>
        /// <returns>The supported context snapshot, which may be empty.</returns>
        PreviewEntryContext LoadContext(PreviewTranslationEntry entry, CancellationToken cancellationToken);

        /// <summary>
        /// Persists staged targets through the format-specific writer and backup boundary.
        /// </summary>
        void Save();

        /// <summary>
        /// Writes the staged project to a new destination without replacing an existing file.
        /// </summary>
        /// <param name="path">The new project destination.</param>
        /// <param name="cancellationToken">Cancels work before the atomic final move.</param>
        void Export(string path, CancellationToken cancellationToken);
    }
}
