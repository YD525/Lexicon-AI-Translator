# Screenshot index

The screenshots were captured from the 2.0.0.2 runtime baseline. They are evidence of the current visual language
and information architecture, not target designs.

| File | Observed state |
|---|---|
| `01-transhub-empty.png` | Empty project host and primary file-selection action. |
| `02-transhub-empty-navigation.png` | Empty project host with alternate navigation state. |
| `03-about.png` | About and component-version presentation. |
| `04-dashboard-empty.png` | Empty token dashboard. |
| `05-settings-request-and-key.png` | Proxy and provider credential configuration with empty secret fields. |
| `06-settings-provider-config.png` | Provider prompt configuration. |
| `07-settings-game-config.png` | Game, PEX, and ESP format configuration. |
| `08-settings-ui-config.png` | Appearance, layout, and UI preferences. |
| `09-settings-engine-config-top.png` | Engine preset and translation behavior at the top of the scroll region. |
| `10-settings-engine-config-bottom.png` | Remaining Engine settings and observed long-page scaling pressure. |
| `11-transhub-node-panel.png` | Empty project host with provider pipeline panel. |
| `12-local-terminology-empty.png` | Empty terminology and preprocessing surface. |
| `13-database-viewer.png` | Advanced dictionary database viewer. |
| `14-provider-wizard-step-1.png` | Custom provider wizard type and name step. |
| `15-provider-wizard-step-2.png` | Custom provider request construction step. |
| `16-provider-wizard-test-success.png` | Successful provider test feedback. |
| `17-provider-wizard-test-response.png` | Captured local test response. |
| `18-provider-wizard-step-3.png` | Custom provider response extraction step. |
| `19-about-credits.png` | Credits view and dependency attribution. |
| `20-startup-splash.png` | Startup progress and the version-display inconsistency. |

## State coverage

| State class | Coverage | Follow-up |
|---|---|---|
| Empty | Shell, dashboard, terminology, and provider fields captured. | Keep as regression references. |
| Loading | Startup captured; project loading not safely reproducible with the current reference fixtures. | Capture through the preview project loader. |
| Populated | Settings and provider configuration captured; a populated translation project is missing. | Capture with the approved reference corpus. |
| Error | Provider validation and the database-without-project crash path were exercised during baseline work. | Add stable inline and operation errors as preview workflows are implemented. |
| Unsaved | Source paths and closing behavior are documented, but no safe screenshot exists. | Capture the shell and tab indicators when the preview persistence model exists. |

The image files contain no API keys, proxy credentials, private translated content, or absolute user paths.
