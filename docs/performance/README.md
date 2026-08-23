# Preview performance and large-project validation

This contract defines repeatable Release x64 validation for the preview workflows. Synthetic fixtures contain
no private project data and complement, rather than replace, the licensed reference corpus.

## Workload profiles

| Profile | Entries | Typical use |
| --- | ---: | --- |
| Small | 1,000 | One focused MCM, XML, PEX, or ESP/ESM project |
| Medium | 25,000 | A large single-format mod or mixed translation pass |
| Large | 100,000 | A deliberately demanding mixed-format project |

Generated entries use stable identities, four format labels, placeholders, untranslated targets, and
low-confidence records. Reference-corpus revisions must be recorded separately when parser and export timings
are measured.

## Budgets

Budgets apply on documented desktop hardware after one warm-up run. Every measurement records the source
revision, Release x64 configuration, operating system, processor, logical processor count, and physical memory.

| Interaction | Small | Medium | Large |
| --- | ---: | ---: | ---: |
| Shell visible and interactive | 3 s | 3 s | 3 s |
| Generated project open | 500 ms | 1.5 s | 5 s |
| Reference-corpus project open on SSD | 2 s | 8 s | 30 s |
| Search or filter result | 100 ms | 250 ms | 750 ms |
| Quality validation | 500 ms | 2 s | 7 s |
| Revision comparison | 500 ms | 1.5 s | 5 s |
| Reference-corpus export on SSD | 2 s | 10 s | 45 s |
| Selection or navigation response | 100 ms | 100 ms | 100 ms |
| Cooperative cancellation latency | 250 ms | 250 ms | 250 ms |

Record real parser and export values with the fixture revision and storage type because archive layout and
storage throughput dominate them. UI-thread dispatch gaps should remain below 100 ms while parser, validation,
comparison, provider, or export work is active.

After five open, close, validate, compare, and export cycles, force a full collection only for measurement. The
retained private-byte increase must remain below 64 MiB and the process-handle increase below 16. A repeatable
increase across two fresh processes is a failure even when it remains within those limits.

## Automated measurement

1. Restore dependencies and build `PhoenixTranslator.sln` as Release x64.
2. Close other CPU- or disk-intensive applications and run one warm-up measurement.
3. Run `scripts/Measure-PreviewPerformance.ps1` again. Optionally pass `-ReportPath` to retain the output.
4. Attach the report to the relevant issue or pull request. Do not commit machine-specific reports.

The runner measures generated-project open, filtering, quality validation, and stable-identity revision
comparison for every profile. It exits with a failure when a budget is missed. Normal regression tests also
verify that validation and comparison honor a pre-cancelled token.

## UI and lifecycle validation

Use Windows Performance Recorder or the Visual Studio profiler for operations that miss a budget. Separate UI
thread activity from parser, provider, storage, and comparison work before assigning ownership. During each
profile:

1. Open and navigate the project, then search and change filters while background work is active.
2. Cancel translation, validation, comparison, and export after work has started and record response latency.
3. Confirm progress remains visible, no partial translation or export is committed, and the prior usable state
   remains available.
4. Repeat five complete cycles and record private bytes, managed heap size, process handles, and UI-thread gaps.
5. Capture a profile for every missed budget and create a focused follow-up in the repository that owns the
   measured bottleneck.

Large preview entry, review, finding, history, comparison, provider-pipeline, and diagnostics collections use
WPF recycling virtualization with logical scrolling. Do not place these controls inside an unbounded vertical
panel, because doing so disables effective container virtualization.
