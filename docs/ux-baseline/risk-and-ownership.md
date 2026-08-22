# Technical risk and ownership map

## Translator risk map

| Area | Risk | Evidence | Direction | Priority |
|---|---|---|---|---|
| `PhoenixGui` | Shell, project tabs, settings, navigation, logging, window lifecycle, and provider setup share a 1,900-line code-behind class. | Direct construction of workflow views, many control event handlers, global state access, and manual visual-state changes. | Introduce a shell view model, navigation contracts, project host, and settings model incrementally. Keep legacy pages routable. | High |
| `TranslateView` | Translation orchestration, row editing, caches, file update, import/export, context tools, history, progress, and window management share a 2,300-line user control. | Direct file dialogs, threads, dictionaries, provider calls, child windows, and mutable UI state. | Extract commands and operation services by workflow. Build the preview workspace beside the legacy view. | Critical |
| Settings | Immediate event-driven persistence can create partial or invalid configurations; credentials share the same surface as ordinary preferences. | `PhoenixGui` text and selection handlers plus provider-specific controls and wizard state. | Use a staged settings model with validation, Apply/Cancel, secret-safe provider tests, and intent-based sections. | High |
| History | History operations are UI-owned and review or provenance semantics are not a durable cross-workflow contract. | `HistoryWindow` directly restores, deletes, and marks current values. | Define Engine provenance and review contracts first, then adapt legacy history and build the preview workflow. | High |
| Provider configuration | Provider types, request templates, response mappings, testing, and node enablement are split across three surfaces. | `PlatformConfigStyleWin`, `NodeStyleWin`, and `CustomWizard` share global configuration and provider concepts. | Define one Provider settings model; retain pipeline construction as Advanced and make tests cancellable. | High |
| Threading | Raw threads and dispatcher calls make cancellation, exception propagation, and lifecycle ownership inconsistent. | Startup, translation configuration, conversion, tracking, and translation view code paths. | Move long-running work behind task-based, cancellation-aware services as each workflow is replaced. | High |
| Error presentation | Modal messages and custom dialogs do not provide one safe, localized error boundary. | `MessageBox`, `MessageBoxExtend`, local status labels, and direct exception text. | Add a shared notification and dialog boundary with stable message identifiers and sanitized details. | High |
| Window coupling | Follower windows track owner location and lifecycle manually. | `CodeView`, `RecordTracking`, and `HistoryWindow`. | Prefer docked context and workflow panes; keep optional detached windows only where power users benefit. | Medium |
| Scaling | Current default size is 1000 by 850 and several settings captures exceed the viewport. | Fixed dimensions, dense forms, and content clipping observed at current sizes. | Design for 1100 by 700 DIPs, scrolling content regions, keyboard focus visibility, and 100 to 200 percent scaling tests. | High |
| Visible text and styling | Literal English strings, icons, colors, and dimensions are distributed across XAML and code. | Surface inventory and screenshots. | Establish semantic resources in #27 and stable English message identifiers in #28 before shell implementation. | High |
| Version display | Splash and About show component versions that do not match the Translator assembly and release version. | Runtime captures show main program `5.1.5.9` while the release baseline is `2.0.0.2`. | Define one authoritative product-version source and list dependency versions separately. | Medium |
| Database tool | The advanced SQLite editor exposes direct query and mutation behavior close to primary terminology tasks. | `DataBaseView` query, edit, and delete commands. | Keep the guarded tool Advanced; provide safe terminology commands for normal use. | Medium |

## Cross-repository ownership

| Concern | Owner | Consumer | Contract boundary |
|---|---|---|---|
| WPF shell, navigation, dialogs, keyboard routing, view state, preview fallback | Phoenix Translator | End user | No WPF types cross into supporting repositories. |
| Project orchestration across supported formats | Phoenix Translator | Workflow views | Calls narrow reader, interface, and Engine services and maps results to UI-safe models. |
| Translation providers, cancellation, persistence, terminology, provenance, review state, comparison, and quality aggregation | Phoenix Engine | Phoenix Translator | Framework-compatible, UI-independent C# contracts with documented persistence versions. |
| PEX semantic context, stable script/string identity, technical-string analysis | PexInterface | Engine and Translator | Managed contracts over released PexReader ABI; no UI ownership. |
| PEX binary parsing, writing, structural diagnostics, and native ownership | PexReader | PexInterface | Versioned C ABI and matching managed interop release. |
| ESP/ESM parsing, stable record metadata, and structural diagnostics | EspReader | Translator and Engine comparison | Versioned C ABI with bounded, documented diagnostics. |

## Dependency sequence

1. Complete Translator UX baseline, design resources, and message vocabulary.
2. Release PexReader or EspReader diagnostics only when a target workflow needs them.
3. Release PexInterface semantics after its required PexReader revision.
4. Release Phoenix Engine provenance, review, comparison, and finding services against compatible dependencies.
5. Advance Translator dependency paths and versions only after the supporting releases exist.
6. Keep preview and legacy consumers compatible until each workflow readiness gate is accepted.

## Security and privacy constraints

- Never log API keys, authorization headers, cookies, custom-provider payload secrets, or proxy passwords.
- Do not include absolute user paths or private source and target text in telemetry or issue artifacts.
- Bound imported files, database queries, provider responses, and parser diagnostics.
- Preserve ordinary TLS validation and make provider tests explicitly user initiated.
- Treat project files, XML, JSON, archives, database contents, and native parser output as untrusted input.
