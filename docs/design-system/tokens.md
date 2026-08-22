# Token catalogue

## Color

| Semantic token | Value | Use |
|---|---:|---|
| `SurfaceCanvas` | `#1A1A1A` | Deep workspace and editor background. |
| `SurfacePrimary` | `#242424` | Main shell and page background. |
| `SurfaceRaised` | `#2A2A2A` | Cards, navigation, dialogs, and tool panes. |
| `SurfaceInteractive` | `#303030` | Resting interactive controls. |
| `Border` | `#3D3D3D` | Ordinary separation. |
| `BorderStrong` | `#555555` | Active boundaries and high-density separators. |
| `TextPrimary` | `#F2F2F2` | Primary text. |
| `TextSecondary` | `#BFBFBF` | Labels and supporting values. |
| `TextMuted` | `#888888` | Hints, metadata, and unavailable context. |
| `AccentGold` | `#FAE306` | Phoenix identity, selected navigation, and primary action. |
| `AccentGoldSoft` | `#F7F1BA` | Selected-row and subtle identity highlight. |
| `ActionBlue` | `#0B74D1` | Links and secondary action emphasis. |
| `FocusBlue` | `#4D8CF7` | Keyboard focus and information. |
| `Success` | `#55B879` | Completed and valid. |
| `Warning` | `#FFB024` | Actionable warning. |
| `Error` | `#E05D5D` | Failed or blocking. |

Status colors require a text label or icon and must meet readable contrast against their surface.

## Typography

| Token | Value | Use |
|---|---:|---|
| `FontFamilyUi` | Segoe UI | Native Windows UI text and reliable .NET Framework rendering. |
| `FontFamilyCode` | Cascadia Mono, Consolas | Code, technical identifiers, and aligned values. |
| `FontSizeCaption` | 11 DIP | Dense metadata only. |
| `FontSizeBody` | 13 DIP | Default controls and content. |
| `FontSizeBodyStrong` | 14 DIP | Emphasized labels and card titles. |
| `FontSizeSection` | 18 DIP | Page sections. |
| `FontSizeTitle` | 24 DIP | Workflow title. |

Body copy uses normal weight. Selected navigation, card titles, and critical values use SemiBold. All caps are
reserved for short telemetry labels and never used for sentences.

## Spacing and shape

The spacing scale is `4, 8, 12, 16, 24, 32` DIPs. Layout uses only scale values unless alignment with a
one-pixel stroke requires a half-DIP at 100 percent scaling.

Radii are 3 DIPs for compact indicators, 5 DIPs for controls, and 8 DIPs for cards or dialogs. Borders remain
one DIP. Elevation is represented primarily by nested surfaces and borders; shadows are limited to detached
dialogs and popups.

## Layout

- Navigation rail: 216 DIPs expanded; 56 DIPs collapsed.
- Context inspector: 320 DIPs default; resizable from 260 to 480 DIPs.
- Compact control height: 30 DIPs.
- Comfortable control height: 38 DIPs.
- Page padding: 24 DIPs compact and 32 DIPs comfortable when width permits.
- Grid rows: 30 DIPs compact and 40 DIPs comfortable.

## Iconography

Use monochrome geometric path icons on a 16 or 20 DIP grid with 1.5 DIP optical stroke. Icons label familiar
actions only; uncommon or destructive actions retain visible text. Do not use emoji glyphs because shape and
baseline vary across supported Windows versions.
