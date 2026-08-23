# Phoenix semantic icon system

## Source and ownership

Phoenix Translator uses a bounded Regular subset of [Microsoft Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons).
The exact upstream revision, source-font checksum, generated subset checksum, and generator version are recorded in
`Phoenix_Translator/ThirdParty/FluentSystemIcons/SOURCE.json`. The redistributed subset retains the upstream MIT
license and notice. No icon asset or metadata is loaded from the network at runtime.

The application uses semantic names such as `Projects`, `Quality`, and `Database`. XAML never depends on an upstream
glyph name or codepoint. `PreviewIconRegistry.g.cs` is the generated boundary between those stable application names
and the pinned Fluent source.

## Inventory and migration decisions

| Existing treatment | Decision | Semantic replacement |
| --- | --- | --- |
| Text-only primary navigation | Add one consistent leading icon while retaining every visible label | Projects, Translate, Review, Quality, History, ProjectUpdate, Settings, AdvancedTools |
| Text `×` in the Phoenix dialog close button | Replace | Dismiss |
| Hand-authored green status ellipse | Replace | Success |
| Advanced Tools category text | Add icons while retaining labels | Provider, Database, Telemetry, Preset |
| Animated splash ellipses | Retain | Progress animation, not a command or semantic status icon |
| Charts and content imagery | Retain | Data visualization, not interface iconography |
| Legacy-only glyphs and bitmap treatments | Defer | Replace when the owning legacy surface is migrated or retired |

The complete registered catalogue additionally reserves semantic actions and states required by migrated workflows:
Open, Add, Remove, Save, Export, Import, Search, Filter, Warning, Error, Information, MoveUp, MoveDown, Apply, Pause,
Resume, Refresh, and Document.

## Phoenix tokens and interaction states

Icons inherit foreground color and enabled opacity from the owning Phoenix control. They do not introduce Fluent
colors or control chrome.

| State | Phoenix treatment |
| --- | --- |
| Normal | Secondary text color in navigation; owning control foreground elsewhere |
| Hover | Owning control interactive surface and border tokens |
| Selected | Primary text color, Phoenix gold selection mark, Regular glyph retained |
| Pressed | Owning control canvas surface token |
| Disabled | Owning control opacity token; meaning remains visible in text |
| Warning | Phoenix warning brush plus visible warning text |
| Error | Phoenix error brush plus visible error text |
| Success | Phoenix success brush plus the persistent status label |

Regular and Filled variants are not mixed implicitly. The first migration intentionally uses Regular icons everywhere;
a future Filled state must be added as an explicit semantic mapping and included in the deterministic subset.

## Accessibility and scaling

`PreviewIcon` is excluded from the automation tree because it decorates visible text or a parent control with an
accessible name. It is never the only cue for a destructive action, severity, navigation destination, or workflow
state. If the embedded font resource is unavailable, each semantic name resolves to a short text fallback.

The control uses font sizing instead of fixed bitmap dimensions, so it remains sharp from 100% through 200% display
scaling and follows WPF text scaling. Containers provide enough width for the supported 16, 20, and 24 DIP tokens.

## Regeneration

Run from the repository root:

```powershell
.\scripts\Generate-FluentIconSubset.ps1 -ProjectRoot .\Phoenix_Translator
.\scripts\Verify-PreviewIcons.ps1 -ProjectRoot .\Phoenix_Translator
```

The generator downloads only the pinned source revision, installs pinned `fonttools` into a temporary directory,
subsets the declared manifest, writes the generated registry and source record, and removes its temporary directory.
Change the revision or manifest deliberately and review the regenerated binary, registry, source checksums, license,
and notice together.
