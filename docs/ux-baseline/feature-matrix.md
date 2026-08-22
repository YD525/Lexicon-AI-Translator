# Current feature matrix

## Disposition vocabulary

- **Retain** preserves the capability and gives it a defined place in the target workflow.
- **Consolidate** combines overlapping surfaces or actions behind one workflow.
- **Relocate** keeps the capability but moves it to a clearer destination.
- **Advanced** keeps the capability behind progressive disclosure.
- **Removal candidate** requires usage evidence and explicit approval before deletion.

No current capability is approved for removal by this baseline.

## Interactive surfaces

| ID | Surface | Current responsibility and primary actions | Current states and data source | Code owner | Target disposition |
|---:|---|---|---|---|---|
| 01 | `SplashWindow` | Starts the application, reports module loading, shows the displayed version, and permits close. | Launching, module status, startup failure, closing; startup thread and loaded module versions. | Translator | Retain as short startup feedback; route failure to a recoverable startup error. |
| 02 | `PhoenixGui` | Hosts project tabs, file selection and drop, global navigation, logs, settings, node panel, About, and dashboard. | No project, project tabs, settings page, About, dashboard, node panel, log visibility, closing; `ModFile`, global settings, provider configuration, and module versions. | Translator | Retain and refactor into the preview shell. Split navigation, settings, project hosting, and status into testable components. |
| 03 | `MainChart` | Displays current and total token usage and provides pause and clear commands. | Empty, collecting, paused, cleared; translation token counters. | Translator with Engine metrics | Relocate to project status or an advanced telemetry panel. |
| 04 | `TranslateView` | Provides the main record list, source and target editors, search, navigation, batch and single translation, caches, import, export, save, history, replacement, conversion, NPC lookup, and database access. | Empty, loaded, filtering, translating, cancelled, cache prompt, edited, saveable, failed; `ModFile`, `P_String`, translation providers, dictionaries, caches, and history. | Translator integration; Engine operations; reader-derived records | Retain as capability, rebuild as the preview workspace, and decompose by command and state ownership. |
| 05 | `RowStyleWin` | Renders and edits one translation entry, persists edits, and applies row status colors. | Selected, editing, translated, unchanged, history-bearing; `P_String` and dictionary updates. | Translator | Consolidate into a virtualized workspace row/editor view model. |
| 06 | `PlatformConfigStyleWin` | Edits cloud, local, and traditional provider settings; adds and removes keys or models. | Provider available, missing key, free provider, selected model, local port; global provider configuration. | Translator settings; Engine provider definitions | Relocate to intent-based Provider settings. Credentials remain outside diagnostics and screenshots. |
| 07 | `NodeStyleWin` | Enables, disables, groups, and adds translation engine nodes. | Enabled, disabled, empty category, counts; configured engine-node collection. | Translator | Retain as an Advanced provider pipeline view. |
| 08 | `CustomWizard` | Creates and tests custom remote or local providers across request, response-mapping, and completion steps. | Step 1 to 3, incomplete input, test running, test success, response mismatch, request failure; custom provider configuration and test response. | Translator with Engine provider contracts | Retain as an Advanced wizard with cancellable tests and safe validation messages. |
| 09 | `TranslateConfig` | Preprocesses text, maintains the local terminology dictionary, detects languages, imports and exports tables, and opens the database. | Empty dictionary, populated dictionary, importing, exporting, detection result, validation failure; `AdvancedDictionary`, language detector, and project languages. | Translator with Engine dictionary services | Consolidate terminology management into the project workflow; relocate global import and export to Settings or Tools. |
| 10 | `DataBaseView` | Executes SQL-like queries, edits rows, deletes selections, and offers query completion. | No project database, query ready, querying, results, editing, empty results, query error; SQLite-backed advanced dictionary. | Translator | Advanced. Keep a guarded database tool separate from the primary terminology workflow. |
| 11 | `MessageBoxExtend` | Presents custom confirmation and cancellation prompts. | Informational, confirm, cancel, caller-supplied colors; caller-owned message data. | Translator | Consolidate into a shared dialog and notification service with stable message identifiers. |
| 12 | `CGView` | Displays the current image or visual asset in a dedicated window. | No image, image shown, closed; caller-provided image source. | Translator | Advanced. Relocate to a context preview pane where supported by the project format. |
| 13 | `PhoenixTranslatorRadarChart` | Visualizes translation preset trade-offs across quality, speed, resource overhead, automation, and manualization. | Named preset and custom values; translation preset profile. | Translator | Relocate into Engine settings as a read-only preset explanation. |
| 14 | `CodeView` | Displays generated or decompiled code, follows the main window, searches text, and selects related lines. | Loading, code shown, match selected, no match, closed; PEX semantic/decompiled text. | Translator display; PexInterface semantics | Advanced. Dock as a project context inspector instead of a follower window. |
| 15 | `HistoryWindow` | Searches history, navigates to entries, restores values, marks current, deletes entries, and clears history. | Empty, populated, filtered, current, restored, deleted, failed; translation-entry history. | Translator today; Engine persistence target | Retain as the History workflow with durable provenance and review semantics. |
| 16 | `InteractiveView` | Copies a provider request and applies a manually supplied response. | Waiting for request, request copied, response entered, applied, closing; interactive provider request and response. | Translator with Engine provider operation | Retain as an interactive provider panel within the translation workflow. |
| 17 | `NPCFinder` | Filters NPC names and navigates to associated dialogue. | Empty input, suggestions, selected NPC, no match; project dialogue metadata. | Translator with EspReader-derived identity | Consolidate into project search and record context. |
| 18 | `RecordTracking` | Shows related NPCs, text entries, books, and dialogue scenes and navigates to a related card. | Loading sections, expanded, collapsed, empty section, selected relation; record relationships from Translator and parser analysis. | Translator integration; EspReader and PexInterface diagnostics | Retain as an Advanced context inspector with asynchronous loading. |
| 19 | `ReplaceWin` | Replaces matching text within a selected scope and records changes. | Input incomplete, ready, replacement applied, no matches; current project entries and history. | Translator | Consolidate into workspace Find and Replace with preview and undo. |
| 20 | `SearchText` | Searches within the code document and supports keyboard submission. | Empty query, next match, no match; `CodeView` text document. | Translator | Consolidate into the Code context inspector. |
| 21 | `TraditionalConvert` | Converts the current entry or all entries between writing variants. | Current or all scope, running, completed, failed; selected or complete translated text collection. | Translator with external conversion provider | Relocate to workspace Tools with explicit scope, preview, cancellation, and undo. |

## Cross-surface findings

- `PhoenixGui` and `TranslateView` contain most orchestration and directly create many secondary windows.
- Settings mix credentials, provider choice, file parsing, UI, and translation behavior in one navigation level.
- History and edited state exist, but review, approval, provenance, and export readiness are not one durable model.
- Several operations use raw threads and dispatcher calls, making cancellation and failure ownership inconsistent.
- Advanced tools are discoverable mainly through local buttons or context menus rather than a stable command model.
- Multiple surfaces use literal English copy and view-specific colors or dimensions.
