# Phoenix Translator UX baseline

This baseline describes Phoenix Translator before the preview redesign. It combines direct observation of
version 2.0.0.2 with a source audit of every interactive WPF surface on the `Remake` branch.

## Artifacts

- [Feature matrix](feature-matrix.md) inventories all 21 interactive surfaces.
- [Workflow maps](workflows.md) compare the current and target paths for eight product workflows.
- [Terminology and states](terminology-and-states.md) defines the English vocabulary used by later issues.
- [Risk and ownership map](risk-and-ownership.md) identifies technical boundaries and cross-repository owners.
- [Screenshot index](screenshots/README.md) records the captured views and state coverage.

## Baseline decisions

- Phoenix Translator remains the integration and UX owner.
- WPF, .NET Framework 4.8.1, and C# 7.3 remain binding constraints.
- The charcoal, blue, yellow or gold, geometric, compact studio language is retained.
- A project-centered shell replaces view-centered navigation incrementally.
- Legacy workflows remain available until their individual preview readiness gates pass.
- Primary actions must fit at 1100 by 700 effective WPF DIPs.
- Long-running operations expose progress, cancellation, and a recoverable failure state.
- Visible English terms in this baseline are product vocabulary, not final message identifiers.

## Evidence method

The inventory was derived from XAML roots, named controls, event wiring, code-behind dependencies, window
construction sites, and the captured runtime views. A surface counts as interactive when it is a `Window` or
`UserControl` that accepts input, triggers an action, or communicates operation state. Resource dictionaries,
`App.xaml`, and non-interactive templates are excluded.

The screenshot set contains no credentials, translated private content, or absolute user paths. States that
require a deterministic populated project are documented from their code paths but are not represented as
runtime screenshots yet. The reference corpus will supply those captures when the relevant preview workflow
can load it safely.
