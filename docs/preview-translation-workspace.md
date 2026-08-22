# Preview translation workspace

The Translate destination now owns a project-centered preview workflow. The complete legacy workspace remains
available through the shell action and `Ctrl+L` while reference projects are compared during preview acceptance.

## Project boundary

`PreviewTranslationProject` normalizes ESP, ESM, ESL, PEX, MCM TXT, XML, and Phoenix RamCache JSON records into
UI-independent entries. Paths remain inside the parser and persistence boundary; the shell receives only the safe
file name. Inputs are rejected when the file is missing, the format is unsupported, the file exceeds 512 MiB, or
the normalized project exceeds 500,000 entries.

The adapter reuses the existing format readers, writers, translation provider pipeline, backup behavior, and
translation memory. A successful save reloads the file through its parser before editing continues. Failures retain
staged edits and expose only registered user-safe messages.

## Workspace state

`PreviewTranslationWorkspaceViewModel` owns search, state and type filters, selection, staged target text, bounded
progress, and cooperative cancellation. Provider calls and parser or writer work run away from the WPF UI thread.
Filtered results are replaced as one collection snapshot so large projects do not generate one UI notification per
entry. The WPF list uses recycling virtualization and keeps technical keys and confidence metadata in the context
inspector rather than the primary editing surface.

## Keyboard map

- `Ctrl+O`: open a supported project.
- `Ctrl+S`: save and reload the current project.
- `F6`: translate the selected entry.
- `Ctrl+Shift+T`: translate the current filtered scope.
- `Esc`: request cancellation of active provider work.
- `Ctrl+L`: open the complete legacy workspace.

Provider results are staged as modified targets. Partial batch failures keep successful results, preserve failed
entries, and report a bounded warning summary.
