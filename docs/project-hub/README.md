# Preview Project Hub

The Projects destination owns project opening and recent-project discovery without creating a second project model.
Every successful open flows into the existing normalized translation workspace, which remains the shared source for
Translate, Review, Quality, History, and Project update.

## Project states

| Current state | User action | Result |
| --- | --- | --- |
| No project | Open, reopen, or drop one supported file | Validate and load the file away from the UI thread |
| Usable project, clean | Open another project | Replace the active normalized project after successful parsing |
| Usable project, modified | Open another project | Require explicit confirmation before discarding staged changes |
| Loading | Any additional open request | Ignore the request until the active load completes |
| Load failure | Dismiss the safe error | Preserve the previously usable project and its staged state |
| Load success | Continue | Enter Translate with all project-backed workflows using the same entries |

## Recent-project boundary

The per-user recent list stores at most 12 private absolute paths and UTC timestamps under local application data.
Only file name, format, localized last-opened time, and availability are exposed to the WPF view. Missing files remain
visible until explicitly removed, and removing a recent reference never deletes project content.

The XML store is limited to 1 MiB, prohibits DTD processing and external resolution, ignores malformed entries, and
replaces its file atomically. It contains no source text, target text, credentials, or diagnostic payloads.

## Keyboard and pointer paths

- `Ctrl+1`: open Projects.
- `Ctrl+O`: select a supported project.
- `Enter`: open the selected available recent project.
- `Delete`: remove the selected recent reference.
- Drop exactly one supported file anywhere on the Project Hub.
- `Ctrl+L`: open the workflow-specific legacy fallback during preview.
