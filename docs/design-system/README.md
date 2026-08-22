# Phoenix WPF design system proposal

This proposal converts the established Phoenix visual language into semantic resources for the preview
workflows. It preserves the charcoal studio surfaces, gold identity accent, blue interaction feedback,
geometric shapes, and compact information density observed in the UX baseline.

## Deliverables

- [Token catalogue](tokens.md)
- [Control-state catalogue](control-states.md)
- [Density, scaling, accessibility, and motion rules](density-scaling-and-accessibility.md)
- [Workflow wireframes](wireframes/README.md)
- [Brand-aligned shell, workspace, and settings direction](hifi/README.md)
- Compiled WPF resources in `Themes/PreviewDesignTokens.xaml` and `Themes/PreviewControlStyles.xaml`

The preview dictionaries are compiled but not merged into `App.xaml`. They establish names, framework
compatibility, and representative controls without changing any legacy runtime view. Each preview workflow
opts in explicitly after its design and readiness gate is accepted.

## Design principles

1. Gold identifies Phoenix and the primary action; it is not a general status color.
2. Blue communicates focus, links, selection support, and information.
3. Status is expressed by icon, label, and color together.
4. Project identity, unsaved state, progress, and warnings remain visible across workflows.
5. Keyboard focus is never removed and does not depend on hover.
6. Compact density is the default; Comfortable changes spacing and target size, not information architecture.
7. Primary actions fit at 1100 by 700 effective WPF DIPs.
8. Advanced tools remain available through predictable progressive disclosure.
