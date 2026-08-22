# Current and target workflow maps

These maps define information flow and user-visible states. They do not prescribe final control layout.

## 1. Open a project

| Current | Target |
|---|---|
| Start application -> wait for splash -> reach empty `PhoenixGui` -> select or drop one or more files -> create tabs and `TranslateView` instances -> parse each file -> show rows or a modal failure. | Start application -> show shell -> choose recent, browse, or drop -> validate format and dependencies -> show cancellable loading state -> open a project workspace -> summarize warnings and unsupported content without discarding valid records. |

Target states: `NoProject`, `Opening`, `Ready`, `ReadyWithWarnings`, `OpenFailed`, `Cancelled`.

## 2. Translate

| Current | Target |
|---|---|
| Select a row -> edit or invoke one-record translation -> optionally choose quick or normal mode -> apply result -> move next; or start batch translation -> watch a local progress bar -> stop or cancel -> save separately. | Select an entry or filtered set -> choose provider and scope -> review cost/context summary -> start a cancellable operation -> stream progress and per-entry outcomes -> keep edits as Draft -> allow retry for failed entries -> save project state independently from export. |

Target states: `Untranslated`, `Draft`, `Translated`, `TranslationFailed`, plus operation states `Queued`, `Running`,
`Cancelling`, `Completed`, `CompletedWithFailures`, `Cancelled`.

## 3. Review

| Current | Target |
|---|---|
| Compare source and target manually, use row colors and history, edit text, and navigate entries. There is no single durable review decision. | Filter Draft or Translated entries -> inspect source, target, context, provenance, and warnings -> edit if needed -> mark Reviewed -> Approve, Reject, or return to Draft -> persist the decision and reviewer-relevant timestamp. |

Target states are proposals for the Engine contract: `Draft`, `Translated`, `Reviewed`, `Approved`, `Rejected`.
The exact transition rules belong to `YD525/Phoenix-Engine#6`.

## 4. Quality assurance

| Current | Target |
|---|---|
| Use local checks, database lookup, code view, record tracking, and manual inspection in separate windows. Failures are not collected into one project result. | Run validation -> collect parser, placeholder, terminology, untranslated, technical-string, and consistency findings -> group by severity and entry -> navigate from a finding to the workspace -> acknowledge or resolve -> compute export readiness. |

Target states: `NotValidated`, `Validating`, `Passed`, `PassedWithWarnings`, `Failed`, `ValidationCancelled`.
Finding states: `Open`, `Acknowledged`, `Resolved`, `SuppressedByRule`.

## 5. Export

| Current | Target |
|---|---|
| Save the active file or choose DSD or RamCache export from the workspace; prompts and format-specific behavior vary. | Choose Export -> show target format, destination, readiness, and unresolved findings -> require an explicit override for allowed warnings -> write to a temporary target -> validate output -> replace final output atomically -> show a safe summary and reveal destination. |

Target states: `CheckingReadiness`, `Blocked`, `Ready`, `Exporting`, `Exported`, `ExportFailed`, `Cancelled`.

## 6. History

| Current | Target |
|---|---|
| Open `HistoryWindow` -> search or filter -> go to, restore, set current, delete, or clear records. | Open project History -> filter by entry, operation, origin, or decision -> compare revisions -> restore as a new revision -> retain provenance -> navigate back to the entry. Destructive deletion is separate from ordinary restore. |

Target states: `Empty`, `Loading`, `Ready`, `Filtered`, `RestorePending`, `RestoreFailed`.

## 7. Update a mod

| Current | Target |
|---|---|
| Invoke update behavior from the active translation view -> select or derive a newer file -> match records -> update the active collection -> inspect effects through ordinary editing and history paths. | Choose Update Project -> select new source -> parse and compare stable identities -> summarize added, removed, changed, and ambiguous records -> resolve ambiguous matches -> apply as one recoverable operation -> retain previous revision and review decisions where identity is stable. |

Target states: `SelectingSource`, `Comparing`, `NeedsResolution`, `ReadyToApply`, `Applying`, `Applied`,
`UpdateFailed`, `Cancelled`. Comparison services belong to `YD525/Phoenix-Engine#7`; stable identities and diagnostics
belong to the relevant reader and interface issues.

## 8. Configure the application

| Current | Target |
|---|---|
| Open Settings -> choose Request and API Key, AI, Game, UI, or Engine tabs -> edit controls that save through several event handlers -> manage nodes or open the custom provider wizard separately. | Open Settings -> choose an intent: Providers, Translation behavior, Project formats, Appearance and accessibility, or Advanced -> edit a staged model -> validate locally -> test providers explicitly -> Apply or Cancel -> report restart requirements and never expose stored secrets. |

Target states: `Clean`, `Modified`, `Validating`, `Invalid`, `TestingProvider`, `TestSucceeded`, `TestFailed`,
`Applying`, `ApplyFailed`, `RestartRequired`.

## Shared navigation model

```text
Shell
|- Project picker / recent projects
|- Workspace
|  |- Translate
|  |- Review
|  |- Quality
|  |- History
|  `- Project update
|- Export
|- Settings
`- Advanced tools
   |- Terminology database
   |- Provider pipeline
   |- Code and record context
   `- Diagnostics
```

The shell owns location, project identity, unsaved state, global progress, warnings, and notifications. Workflow
views own their local selection and commands. Engine and reader services own domain state and diagnostics, never
WPF navigation.
