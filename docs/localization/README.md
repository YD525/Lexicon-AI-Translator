# Preview message catalogue

Phoenix Translator uses the compiled `Properties/Resources.resx` file as the English source catalogue for all
redesigned preview workflows. Stable identifiers describe product intent rather than English wording or control
position.

## Identifier convention

Identifiers use PascalCase segments separated by underscores:

`Area_Feature_Element_Purpose`

Examples:

- `Shell_ProjectOpen_Title`
- `Workspace_Entry_TargetText_Label`
- `Settings_Providers_Test_Succeeded`
- `Accessibility_Workspace_ContextInspector_Name`

The identifier remains stable when English wording, layout, or control type changes. Avoid directional terms such
as `Left`, visual terms such as `YellowButton`, numbered names, and complete English sentences as identifiers.

## Ownership

- `Properties/Resources.resx` is the authoritative English catalogue.
- Every entry has a translator comment describing location, intent, and placeholders where applicable.
- `PreviewMessageExtension` resolves identifiers in preview XAML.
- `UIManagement/Preview/PreviewMessageExamples.xaml` compiles representative localized controls without loading
  a runtime preview view.
- `PreviewMessageCatalog.Format` formats messages with documented composite-format placeholders.
- `scripts/Verify-PreviewMessages.ps1` rejects literal visible text and unknown identifiers below
  `UIManagement/Preview`.
- The GitHub build runs the verifier before compilation.

Legacy views are migrated only when their workflow enters preview. Existing literal strings outside the preview
directory are intentionally out of scope.

## XAML usage

```xml
<TextBlock Text="{localization:PreviewMessage Shell_ProjectOpen_Title}" />
```

The preview XAML root declares:

```xml
xmlns:localization="clr-namespace:PhoenixTranslator.UIManagement.Localization"
```

## Code usage

Use `PreviewMessageCatalog.Get` for messages without placeholders and `PreviewMessageCatalog.Format` for
composite messages. Do not concatenate localized fragments. Pass complete values such as counts, names, and
durations through documented placeholders.

## Placeholder rules

- Placeholders use zero-based composite-format indexes such as `{0}`.
- Resource comments define the meaning and expected type of every placeholder.
- A sentence owns its punctuation; callers do not append localized punctuation.
- Identifiers with different grammar or plurality remain separate until the exchange format and target language
  rules support an agreed plural model.
- User content is never used as a format string.

## Accessibility text

Accessibility names and help text use the same catalogue with the `Accessibility_` area. An icon-only action must
have a stable accessibility identifier even when its tooltip uses a separate message.

## Weblate exchange

Weblate imports the RESX file with identifiers preserved as source keys and comments preserved as translator
context. A translated resource uses the normal satellite naming convention, for example `Resources.de.resx`.
Before import or export, validation checks unique identifiers, placeholder parity, non-empty values, and source
comments. Generated satellite assemblies remain build artifacts and are not committed.
