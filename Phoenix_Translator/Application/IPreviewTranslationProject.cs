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
        /// Persists staged targets through the format-specific writer and backup boundary.
        /// </summary>
        void Save();
    }
}
