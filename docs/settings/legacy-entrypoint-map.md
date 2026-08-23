# Settings consolidation map

Phoenix Translator currently exposes configuration through several tabs, popovers, dialogs, and project tools.
The preview replaces that distribution with one Settings Center hosted by the application shell. Search, intent
categories, staged edits, and a persistent action bar keep the complete surface understandable without removing
power-user options.

## Central information architecture

| Settings Center category | User intent | Consolidated legacy sources |
| --- | --- | --- |
| General | Choose application-level defaults and startup behavior. | Main Settings shell, game selection, source and target language defaults. |
| Providers | Choose providers, models, credentials, and local endpoints; validate configuration. | `Request & ApiKey`, `PlatformConfigStyleWin`, provider-specific cards. |
| Translation | Control presets, context, language detection, punctuation, placeholders, and preprocessing. | `AI Configs`, `Engine Configs`, translation preset controls, relevant `TranslateConfig` options. |
| Files & formats | Configure game paths, ESP and PEX behavior, code generation, and import or export defaults. | `Game Configs`, format controls, global import and export actions from `TranslateConfig`. |
| History & data | Configure caches, translation memory, automatic database updates, and retention behavior. | Cache controls, database viewer entry points, terminology and history persistence options. |
| Appearance & accessibility | Choose density, text direction, language, and accessible presentation defaults. | `UI Configs`, theme actions, text-layout controls. |
| Advanced | Configure the provider pipeline, custom providers, proxies, concurrency, throttling, and diagnostic behavior. | Node menu, `NodeStyleWin`, `CustomWizard`, proxy controls, advanced Engine settings. |

Terminology entries and database records remain project or data content rather than preference values. The
Settings Center owns their global behavior and offers one route to the relevant manager, but does not mix record
editing into ordinary settings forms.

## Legacy entry points

| Legacy entry point | Current behavior | Preview destination |
| --- | --- | --- |
| Main `Settings` destination | Hosts five implementation-oriented tabs. | Open the Settings Center overview. |
| `Request & ApiKey` tab | Edits proxy values, credentials, provider models, and local ports. | Providers; proxy fields move to Advanced. |
| `AI Configs` tab | Edits the global additional prompt. | Translation. |
| `Game Configs` tab | Mixes file-format and game-specific parser options. | General and Files & formats. |
| `UI Configs` tab | Edits text direction and theme behavior. | Appearance & accessibility. |
| `Engine Configs` tab | Mixes presets, context, concurrency, database, and language behavior. | Translation, History & data, and Advanced. |
| Provider configuration cards | Save model, key, and endpoint changes immediately. | Providers with staged changes and masked credentials. |
| Node menu | Enables or disables providers and preprocessing immediately. | Advanced provider pipeline. |
| Custom provider wizard | Creates request templates and tests calls in a separate window. | Advanced custom-provider editor with an explicit test action. |
| Translation configuration window | Mixes terminology content, preprocessing, languages, and data transfer. | Translation and History & data routes; terminology content remains in its dedicated manager. |
| Database viewer | Opens global terminology data outside a clear settings hierarchy. | History & data route to the data manager. |
| Theme and language event handlers | Persist individual values as soon as controls change. | Appearance & accessibility with Apply, Cancel, and restart guidance. |

## Overview and navigation behavior

- A horizontally scrollable tab row contains the seven stable intent categories without duplicating the shell's
  vertical navigation. Category descriptions remain available as tab tooltips and compact page context.
- A global search matches labels, descriptions, legacy terms, and common synonyms such as `API key`, `node`,
  `dictionary`, `PEX`, `context`, and `theme`.
- Search filters the category tabs by labels, descriptions, legacy terms, and common synonyms.
- Each category starts with a short outcome-oriented description and uses a small number of cohesive cards.
- Advanced dependencies are collapsed by default and explain why unavailable controls are disabled.
- A persistent footer shows `Clean`, `Modified`, or `Invalid` and always keeps Apply, Cancel, and Reset reachable.
- Credentials are write-only: existing values appear only as `Stored securely`; replacement values are never
  copied into diagnostics, notifications, screenshots, or searchable text.
- Provider tests use staged values without saving, send no project or translation text, read no response body,
  and can be cancelled explicitly.
- The legacy settings workspace remains one explicit fallback during preview. It is not presented as another
  primary settings destination.

## Migration rule

Every old settings action must either navigate to one Settings Center category, navigate to a content manager, or
be documented as intentionally retired. No new configuration field may introduce another top-level window or
immediate persistence event.
