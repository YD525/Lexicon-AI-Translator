# Preview review and quality assurance

The preview workflow combines human translation decisions with normalized technical findings while keeping
`Review` and `Quality` as direct shell destinations. Both destinations operate on the same normalized entries as
the translation workspace. `Go to translation entry` clears incompatible translation filters, selects the exact
affected entry, and returns to `Translate`.

## Review model

Review state is independent from translation and persistence state:

- `Unreviewed`: no human decision applies to the current target.
- `Reviewed`: the current target was inspected but has not been approved or rejected.
- `Approved`: the current target is accepted under the active export policy.
- `Rejected`: the current target requires correction.

Editing target content invalidates an existing decision and returns the entry to `Unreviewed`. Provider output,
manual edits, imported targets, and missing targets have distinct provenance labels. Bulk approval operates only
on the visible eligible scope, displays the exact affected count, requires confirmation, and can be undone until
another bulk approval or project change.

Review decisions are stored per user under local application data. The project path identifies the metadata file
only through a SHA-256 fingerprint. Stored XML contains project-local entry keys, review states, target
fingerprints, and acknowledged finding identifiers; it contains no project path, source text, or target text. A
changed target fingerprint prevents a stale decision from being restored.

## Normalized findings

Every finding contains a stable rule identifier, stable project-local identity, severity, diagnostic source,
affected entry, content fingerprint, remediation guidance, and resolution. Editing affected content invalidates a
previous acknowledgement. The initial Translator-owned rules detect:

- untranslated targets;
- placeholder mismatches;
- invalid replacement or control characters;
- inconsistent targets for duplicate sources;
- identifier-, path-, and address-like technical strings;
- low parser-confidence scores.

`Error` findings block export and cannot be acknowledged. `Warning` findings require either correction or an
explicit acknowledgement. Acknowledgement does not change severity. When the underlying condition is corrected,
revalidation removes the finding. Supporting Engine, Interface, and reader contracts can add format-native
findings behind the same normalized model without taking WPF ownership.

## Export readiness

The footer distinguishes four states:

- `Export blocked by errors`;
- `Warnings require review before export`;
- `Ready with acknowledged warnings`;
- `Ready for export`.

The preview computes readiness but does not bypass format writers or silently modify content. The complete legacy
workspace remains available through the persistent shell fallback.

## Keyboard paths

- `Ctrl+3`: open Review.
- `Ctrl+4`: open Quality.
- `Ctrl+Enter`: approve the selected entry.
- `Ctrl+Shift+Enter`: approve the visible review scope after confirmation.
- `Ctrl+Z`: undo the most recent bulk approval.
- `F8`: navigate from the selected finding to its translation entry.
- `Ctrl+L`: open the legacy workspace.

## Validation artifacts

The screenshots in `screenshots` use the deterministic `mixed-review-quality.xml` fixture. They contain no
provider credentials, private translated content, or absolute user paths.
