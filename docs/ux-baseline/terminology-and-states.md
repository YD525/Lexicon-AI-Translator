# English terminology and state model

## Product terminology

| Preferred term | Meaning | Avoid |
|---|---|---|
| Project | One opened source file or coordinated set of files plus translation state and settings. | Mod file when referring to the whole workspace |
| Entry | One translatable unit with stable format identity, source, target, context, and state. | Row as a domain term |
| Source text | Original text read from the project. | From string |
| Target text | Editable translated text. | To string, result text when referring to the editor |
| Provider | A cloud, local, traditional, or interactive translation implementation. | Node when users choose a translation service |
| Provider pipeline | Ordered provider and preprocessing configuration. | Engine nodes outside Advanced settings |
| Terminology | Project or global source-to-target terms and matching rules. | Keywords when entries are full terms |
| Translation memory | Reusable prior translations with origin and language direction. | Cache when users can inspect or manage the records |
| Context | Related records, dialogue, code, metadata, and preceding content supplied for understanding. | Tracking data |
| Finding | A quality or parser diagnostic with severity, location, and resolution state. | Error for warnings or information |
| Revision | One immutable historical version of an entry or project operation. | Backup when it is part of normal history |
| Review | Human assessment of a translation before approval. | Check when a durable decision is meant |
| Approval | Explicit decision that an entry is acceptable for export under current policy. | Complete when only translation finished |
| Export readiness | Result of required validation and review rules for the selected scope. | Save state |
| Project update | Comparison and migration from an older source revision to a newer one. | Reload when identity matching occurs |
| Advanced tools | Expert capabilities that are retained but do not dominate primary workflows. | Hidden tools |

## State dimensions

State is split into independent dimensions. A project may be `Ready`, `Modified`, and `Validating` at the same
time; one overloaded status value must not represent all three.

### Project lifecycle

`NoProject -> Opening -> Ready | ReadyWithWarnings | OpenFailed | Cancelled`

`Ready | ReadyWithWarnings -> Closing -> NoProject`

### Persistence

`Clean -> Modified -> Saving -> Clean | SaveFailed`

Closing a modified project requires Save, Discard, or Cancel. A failed save remains `Modified`.

### Entry translation

`Untranslated -> Draft -> Translated`

An edited generated translation returns to `Draft` until review policy says otherwise. Exact provenance and
review transitions are defined in Phoenix Engine, not inferred from WPF colors.

### Review proposal

`Draft | Translated -> Reviewed -> Approved | Rejected`

`Rejected -> Draft`; a meaningful edit to `Reviewed` or `Approved` content invalidates the decision and returns
the entry to `Draft`. This proposal is input to `YD525/Phoenix-Engine#6`.

### Long-running operation

`Idle -> Queued -> Running -> Completed | CompletedWithFailures | Failed | Cancelling -> Cancelled`

Cancellation is cooperative. `Cancelling` keeps primary destructive actions disabled while preserving visible
progress. Partial results are either committed per documented unit or rolled back; the UI must state which.

### Finding severity and resolution

Severity: `Information`, `Warning`, `Error`.

Resolution: `Open`, `Acknowledged`, `Resolved`, `SuppressedByRule`.

Acknowledgement does not change severity. Only policy determines whether an acknowledged warning permits export.

### Settings

`Clean -> Modified -> Validating -> Clean | Invalid | ApplyFailed`

Provider testing is a nested operation and must not save a key, model, URL, or payload merely because the test
succeeded.

## Empty, loading, failure, and unsaved rules

- Every primary workflow has a useful empty state with one obvious next action.
- Loading replaces neither the shell nor project identity and always exposes cancellation when safe.
- Failure messages state the failed action, preserve recoverable input, and offer retry or a safe exit.
- Unsaved state is visible in the shell and at the affected project tab.
- Warnings do not use the same visual or term as fatal errors.
- Status text never contains credentials, private translated content, or absolute user paths.
