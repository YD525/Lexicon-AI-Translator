# Density, scaling, accessibility, and motion

## Density

Compact is the default for experienced translators and data grids. Comfortable increases control height, row
height, and surrounding spacing by one scale step. It does not hide columns, change command names, or rearrange
workflow ownership.

| Element | Compact | Comfortable |
|---|---:|---:|
| Control height | 30 DIP | 38 DIP |
| Data row | 30 DIP | 40 DIP |
| Card padding | 16 DIP | 24 DIP |
| Page gap | 16 DIP | 24 DIP |
| Minimum pointer target | 30 by 30 DIP | 38 by 38 DIP |

## Scaling matrix

Validate 1100 by 700 effective DIPs at 100, 125, 150, 175, and 200 percent Windows scaling. Also validate
1440 by 900 and 1920 by 1080 effective DIPs. Primary navigation, current project, unsaved indicator, operation
status, editor, and primary action must remain visible at the minimum.

At constrained width, the context inspector collapses before navigation. Navigation then collapses to icons with
accessible names. Primary workflow content never becomes horizontally scrollable; tables may scroll within their
own region.

## Keyboard and assistive technology

- Logical tab order follows navigation, workflow toolbar, content, inspector, and status.
- Every icon-only command has an accessibility name and visible tooltip.
- Focus returns to the invoking control after dialogs and popups close.
- Validation updates use an appropriate live region without repeatedly announcing progress.
- Selection, review state, and findings expose text independent of color.
- Decorative geometry is not exposed to automation peers.

## Contrast

Primary text targets at least 4.5:1 against its surface. Large text and non-text focus or control boundaries target
at least 3:1. Muted text is not used for required instructions or enabled actions. High Contrast mode replaces
semantic brushes through system-aware resources; templates must not encode meaning in bitmap assets.

## Motion

Fast feedback uses 100 milliseconds and ordinary transitions use 160 milliseconds. Motion is limited to state
continuity, progress, pane expansion, and focus context. No looping decorative motion runs while a user is editing.
Reduced-motion mode disables translation and expansion animations without delaying state changes.
