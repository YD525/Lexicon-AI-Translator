# Control-state catalogue

| Control | Required states | Phoenix treatment |
|---|---|---|
| Primary button | Default, hover, pressed, focused, disabled, busy | Gold fill; dark text; blue two-DIP focus ring; busy keeps label and adds progress. |
| Secondary button | Default, hover, pressed, focused, disabled | Raised surface; strong border; gold hover border; blue focus ring. |
| Destructive button | Default, hover, pressed, focused, disabled, confirm | Neutral surface with red label and border; destructive fill appears only in confirmation. |
| Text input | Empty, populated, hover, focused, invalid, disabled, read-only | Canvas surface; blue focused border; error border plus adjacent message; read-only remains legible. |
| Search input | Empty, query, focused, no results, results | Search icon, clear command, result count, and keyboard hint where useful. |
| Combo box | Closed, hover, focused, open, selected, disabled | Same field chrome as text input; popup uses Raised surface and visible selection. |
| Check box | Off, on, mixed, hover, focused, disabled | Standard check geometry with text label; never a toggle switch for multi-selection. |
| Toggle | Off, on, hover, focused, disabled | Track and thumb plus persistent label; on state uses gold. |
| Navigation item | Default, hover, current, focused, attention, disabled | Current uses gold marker and strong text; attention adds status badge without replacing current. |
| Tab | Clean, current, modified, hover, focused, closing | Current gold underline; modified dot with accessible label; close is keyboard reachable. |
| Table row | Default, hover, selected, focused, modified, warning, error | Selection and status remain separate; status uses leading icon and text or tooltip. |
| Dialog | Opening, active, validating, busy, failed | Clear title, body, action order, Escape behavior, and default command. |
| Notification | Information, success, warning, error, progress | Icon, heading, concise action, optional safe details; does not steal focus unless blocking. |
| Progress | Determinate, indeterminate, cancelling, completed, failed | Label includes operation and count; cancellation remains visible until acknowledged. |
| Validation | Neutral, validating, valid, warning, invalid | Message is adjacent to the field and announced; color is supplementary. |
| Empty state | First use, no results, unavailable | Explains state, provides one primary next action, and preserves filters for no results. |

## Interaction rules

- Enter invokes the focused primary local action; it never starts a destructive project-wide operation silently.
- Escape closes a popup, cancels a transient mode, or requests cancellation in that order.
- Space operates focused buttons, toggles, and check boxes.
- Ctrl+S saves project state; Ctrl+Shift+S opens explicit export only when assigned and documented.
- Busy controls do not disappear or change position. They disable repeated invocation and expose status.
- Tooltips supplement visible labels and must not contain required instructions.
- Error details are selectable and copyable after secrets and absolute paths are removed.
