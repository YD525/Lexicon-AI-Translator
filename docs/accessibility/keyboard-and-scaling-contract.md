# Keyboard and scaling contract

## Scope

This contract applies to the preview shell and every preview workflow. The legacy workspace remains a fallback and
retains its existing keyboard behavior until each legacy surface is replaced.

## Focus contract

- On startup, focus moves into the first control of the active workflow.
- Direct navigation with `Ctrl+1` through `Ctrl+8` moves focus into the selected workflow instead of leaving it in
  the navigation rail.
- Translation, review, quality, project update, and settings start in their search field. History starts on its
  revision timeline. Placeholder destinations return focus to primary navigation.
- File pickers, confirmations, and keyboard help restore focus to the control that invoked them.
- `Tab` and `Shift+Tab` follow visual reading order. Focus must remain visible through the shared gold focus visual.

## Command map

| Scope | Command | Action |
| --- | --- | --- |
| Global | `Ctrl+1` through `Ctrl+8` | Open a workflow |
| Global | `Ctrl+L` | Open the legacy workspace |
| Global | `F1` | Open the complete keyboard command reference |
| Translation | `Ctrl+O` | Open a project |
| Translation | `Ctrl+S` | Save the project |
| Translation | `F6` | Translate the selected entry |
| Translation | `Ctrl+Shift+T` | Translate the visible scope |
| Translation | `Esc` | Cancel the active operation |
| Review and quality | `Ctrl+Enter` | Approve the selected entry |
| Review and quality | `Ctrl+Shift+Enter` | Approve the visible scope |
| Review and quality | `Ctrl+Z` | Undo the last bulk approval |
| Review and quality | `F8` | Go to the selected entry |
| Project update | `Ctrl+Shift+O` | Compare a previous revision |
| Project update | `Ctrl+Enter` | Reuse the selected translation |
| Project update | `Ctrl+Shift+Enter` | Reuse visible translations |
| Project update | `Ctrl+Z` | Undo the last reuse |
| Project update | `F8` | Go to the selected entry |
| Settings | `Ctrl+S` | Apply valid staged settings |
| Settings | `Esc` | Discard staged settings after confirmation |

The combinations intentionally avoid Windows system shortcuts. `F6` and `F8` are workflow-local and are not used
for shell navigation. Primary visible actions also expose contextual `Alt` access keys.

## Automation and status contract

- Search fields, filters, navigation regions, lists, context panes, and validation regions expose stable localized
  automation names.
- Changing status and validation summaries use polite live-region semantics.
- Notifications include a visible semantic prefix: Information, Success, Warning, or Error. Color reinforces this
  meaning but never carries it alone.
- Item text supplies the accessible name for list entries; icon-only actions are not allowed without an automation
  name and help text.

## Layout and scaling matrix

The shell has an effective minimum size of 1100 by 700 WPF DIPs. Workflow content uses flexible center columns,
bounded side panes, wrapping descriptions, and scrolling settings content so primary actions remain reachable.

| Display scenario | Effective WPF area | Expected result |
| --- | ---: | --- |
| 1280 by 720 at 100% | 1280 by 720 DIPs | Shell opens above its minimum; all primary actions remain visible |
| 1920 by 1080 at 125% | 1536 by 864 DIPs | No clipping; additional workspace area is available |
| 1920 by 1080 at 150% | 1280 by 720 DIPs | Same effective boundary as the 1280 by 720 baseline |
| 2560 by 1440 at 200% | 1280 by 720 DIPs | Same effective boundary as the 1280 by 720 baseline |

Each release candidate must repeat this matrix with keyboard-only traversal and Windows high contrast enabled. Any
new fixed-width pane must fit the 1100 by 700 minimum or provide an explicit scroll boundary.
