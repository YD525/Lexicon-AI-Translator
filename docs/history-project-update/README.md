# Preview history and project updates

The preview combines project history and revision comparison around the active normalized translation project.
`History` presents privacy-preserving events. `Project update` compares a selected prior revision with the current
project and stages only explicit reuse decisions. The complete legacy history and mod-update tools remain
available as workflow-specific fallbacks.

## Revision identity and classification

Comparison uses project-local stable keys and never assumes that source order is stable. Duplicate keys in either
revision are treated as conflicts instead of being silently discarded. Results use six explicit states:

- `Unchanged`: source and target are equal.
- `Added`: the key exists only in the current revision.
- `Removed`: the key exists only in the previous revision.
- `Source changed`: source content changed and the current entry has no reviewed target.
- `Reusable`: source content is unchanged, the current target is empty, and the previous target is available.
- `Conflict`: populated targets differ, reviewed work could be affected, or identity is ambiguous.

Only `Reusable` entries participate in visible-scope reuse. A conflicting prior target remains available for an
individual decision, but replacing the current target requires a warning confirmation. Reuse invalidates the
entry's previous review decision, marks its provenance as `Previous project revision`, and immediately flows into
the existing review and quality analysis.

## Reversibility and persistence

The most recent single or visible-scope reuse can be undone before another reuse action or project change. Undo
restores the staged target and human review state. Project content is persisted only through the existing Save
command and format writer.

Revision events are stored per user under local application data. The project path is converted to a SHA-256 file
identity. Stored XML is bounded to 2,500 recent events and 5 MiB and contains only timestamps, stable action IDs,
bounded project-local identities, safe record metadata, provenance, and SHA-256 content fingerprints. It contains
no absolute project path, source text, or target text. DTD processing is prohibited when history is loaded.

## Keyboard paths

- `Ctrl+5`: open History.
- `Ctrl+6`: open Project update.
- `Ctrl+O`: open the current project.
- `Ctrl+Shift+O`: select and compare a previous revision.
- `Ctrl+Enter`: reuse the selected prior target after any required conflict confirmation.
- `Ctrl+Shift+Enter`: reuse visible safe candidates after confirming the exact count.
- `Ctrl+Z`: undo the most recent reuse action.
- `F8`: reveal the selected current entry in Translate.

## Validation

Deterministic tests cover identical, additive, subtractive, changed, reusable, conflicting, reordered, and
duplicate-identity comparisons. They also verify that classification never changes a reviewed target, explicit
reuse resets review state, and persisted history contains neither project paths nor translation content.
