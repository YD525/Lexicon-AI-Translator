# Preview rollout and legacy retirement

## Purpose

The redesigned workspace is adopted one workflow at a time. A saved rollout choice never removes
legacy code or migrates private project content. Disabled destinations open the compatible legacy
workspace while all other preview workflows retain their own choices.

## Workflow matrix

| Workflow | Preview owner | Compatible fallback | Data boundary |
| --- | --- | --- | --- |
| Project Hub | Project Hub | Legacy dashboard | Existing project files and bounded recent-project metadata |
| Translation | Translation Workspace | Legacy translation view | Shared normalized project entries |
| Review | Review Workspace | Legacy translation view | Shared targets and private review metadata |
| Quality | Quality Workspace | Legacy translation view | Deterministic findings over normalized entries |
| History | History Workspace | Legacy history view | Existing engine translation history and private activity records |
| Project Update | Project Update Workspace | Legacy update flow | Explicit comparison and reuse decisions |
| Settings | Settings Center | Legacy settings windows | Existing engine settings plus versioned rollout state |
| Advanced Tools | Advanced Tools | Legacy provider tools | Provider configuration, bounded database results, and token totals |

Each choice is staged in Settings and applied together with the remaining settings changes. The
rollout file is versioned, written atomically, and recovered from its last known-good backup when
the primary file is corrupt or incompatible.

## Functional replacement coverage

The preview workspace provides direct replacements for project selection, translation, context,
find/replace, terminology, translation-table transfer, RamCache transfer, cache maintenance,
review, quality analysis, translation history, project-update comparison, unified settings,
provider pipelines, custom provider setup, guarded database access, telemetry, diagnostics, and
release information. The database replacement starts read-only, limits SELECT results to 1,000
rows, and requires a separate destructive confirmation before mutation statements can run.

Legacy windows remain available as a controlled fallback. Their presence is not evidence of a
missing preview implementation and they must not be deleted before the retirement gates below are
explicitly accepted.

## Readiness gates

A workflow is eligible for retirement only when all applicable gates have evidence:

- Behavior: required actions, empty/error/unsaved states, keyboard routes, cancellation, and undo
  behavior are verified.
- Data integrity: round trips preserve stable entry identity, malformed input is rejected, writes
  are atomic where applicable, and destructive scope is confirmed.
- Accessibility: keyboard-only use, focus recovery, accessible names, non-color state meaning,
  1100 × 700 DIPs, and 200% scaling are verified.
- Performance: large entry filtering, quality analysis, revision comparison, history loading, and
  result rendering stay responsive and bounded.
- Reference projects: the deterministic mixed synthetic corpus passes and approved external
  reference projects complete the same workflow without redistributed third-party content.

## Release procedure

1. Build Release x64 with Visual Studio 2022 and the .NET Framework 4.8.1 targeting pack.
2. Run the complete preset and preview regression harness plus the performance validation mode.
3. Exercise the mixed reference workflow: open, translate, review, validate, export, inspect
   history, compare an update, transfer RamCache, and reopen the result.
4. Enable one candidate workflow while all remaining choices keep their previous state.
5. Record functional, accessibility, performance, and reference-project evidence in the rollout
   issue before proposing any legacy deletion.
6. Publish the normal project version only after the merge is accepted. Preview-rollout work does
   not require a separate dependency revision.

## Rollback procedure

1. Open Settings > Advanced > Preview workflow rollout.
2. Disable only the affected workflow and apply Settings.
3. Re-enter the destination; the compatible legacy workspace opens without changing other rollout
   choices or project files.
4. If Settings cannot load, close the application and remove only
   `%LOCALAPPDATA%\PhoenixTranslator\preview-rollout.xml`. The next start uses the backup or the
   all-enabled default. Do not remove engine settings or project files.
5. Export privacy-safe diagnostics when available. Diagnostics must not include credentials,
   translated private content, or absolute paths.

## Retirement decision

No legacy view is removed by this rollout. Retirement requires explicit acceptance recorded in the
tracking issue after every readiness gate is complete. A later focused issue must name the exact
legacy types and entry points to remove, preserve a rollback release, and include its own version
update.
