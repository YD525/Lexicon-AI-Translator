# Preview shell

The preview shell is the incremental entry point for the Phoenix Translator UI redesign. It preserves the
existing WPF, .NET Framework 4.8.1, and C# 7.3 runtime while workflow screens are replaced one at a time.

## Startup and fallback

The splash window opens `PreviewShellWindow` after normal application initialization. Every preview destination
currently exposes the complete legacy workspace through a global action and `Ctrl+L`. The legacy window is created
only when requested, reused while open, and remains the functional owner of project workflows until their dedicated
preview issues are completed.

## Navigation

The primary destinations have stable order and keyboard access:

1. Projects (`Ctrl+1`)
2. Translate (`Ctrl+2`)
3. Review (`Ctrl+3`)
4. Quality (`Ctrl+4`)
5. History (`Ctrl+5`)
6. Project update (`Ctrl+6`)
7. Settings (`Ctrl+7`)
8. Advanced tools (`Ctrl+8`)

`PreviewShellViewModel` owns the selected destination, safe project identity, modified state, warning summary,
long-running operation status, and the non-blocking notification boundary. Notifications retain semantic severity,
resolve user-safe text from the source catalogue, and remain dismissible without stealing focus. Workflow
implementations should update shell state rather than introduce local copies in individual views.

## Layout contract

The shell enforces a minimum effective size of 1100 by 700 WPF DIPs. Product identity, project identity, preview
state, version, global fallback, navigation, and operation status remain visible independently of the selected
destination. Visible preview copy is resolved through stable source-catalog identifiers.

## Rollout boundary

The shell establishes information architecture and navigation only. Project opening, translation, review, quality,
history, update, and settings behavior remains in `PhoenixGui` until the corresponding workflow issue supplies a
preview implementation and migration tests.
