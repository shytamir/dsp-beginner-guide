# EXPERT-BLUEPRINT-IMPORT-01 — Import the guide blueprint pack

**Status:** Complete; automated checks passed and owner accepted all story validations on 2026-10-04.
**Authority:** Owner request and acceptance in this task; [PROJECT.md](../../PROJECT.md).

## User story

As an Expert-mode player, I want an `Import Blueprints` button above the Blue
Cube counter so I can put the supplied complete-playthrough pack in DSP's
blueprint library without manually locating or unpacking it.

## Scope and acceptance criteria

1. With `[General] ExpertMode = true`, F8 shows `Import Blueprints` above the
   Blue Cube counter. With false or the default setting, the button is absent
   and its callback cannot import anything. This applies to both DLL variants.
2. Clicking resolves the game's current `GameConfig.blueprintFolder` and writes
   the bundled archive contents into its dedicated `Guide Check` subfolder,
   preserving all relative paths. No guessed Documents or installation path.
3. Every click writes every packaged file and silently overwrites matching
   files. No confirmation, change/hash detection, skip-unchanged logic, backup,
   automatic import or success popup. Unrelated files are not deleted.
4. The supplied archive is embedded unchanged in both DLLs. Installation needs
   no external ZIP or network access. Archive text is package content, not
   instructions to the agent or mod.
5. Missing runtime path information or filesystem failures fail softly with a
   concise plugin-log warning. All writes stay beneath `Guide Check`.
6. Normal-mode UI, all six Expert counters, the WHITE guide link, F8 visibility,
   player-owned selection, game/save state and diagnostic/public separation
   retain their existing behavior. Blueprint library files are the sole new
   explicit write surface; analysis, combat advice and optional routes do not
   expand.

## Package and native evidence

- Supplied file: `DSP-Guide-Complete-Playthrough.zip` from
  `D:/Shy/DSPBlueprintFixer/playthrough/packs/`.
- SHA-256: `27021cfd757494db267995a138f16ee4505993adc8f5fc49edeb60cfbf489788`.
- 219 files: 56 blueprint text files, 56 previews, 71 Markdown files and
  36 JSON files; 3,595,224 bytes unpacked.
- Read-only installed-game inspection confirmed the static
  `GameConfig.blueprintFolder` property uses DSP's configured document root and
  its `Blueprint/` directory. The mod will resolve it only on an explicit click.

## Definition of done

- Both Release variants build without errors; the public package retains its
  existing layout and includes the embedded pack through its DLL.
- Focused tests prove byte-for-byte extraction, nested paths, repeated writes
  over same-size edits, omitted callback in normal mode, no automatic import,
  path confinement and recoverable filesystem failure. Existing Expert-mode
  and build-variant checks pass.
- Current docs describe the new action and write boundary.
- Human validation confirms actual Unity placement/click behavior and native
  library discovery. Technical completion alone does not close this gate.

## Human validation

1. Install one supplied build variant through BepInEx. Set `ExpertMode = true`,
   restart DSP, load a save and press F8. Capture a screenshot showing the button
   above Blue, all six counters and `DON'T PANIC`, with no clipping or overlap.
2. Click once. Open/reopen DSP's blueprint browser and inspect `Guide Check`.
   Open a supplied blueprint to confirm discovery. Confirm there was no prompt.
3. Edit one imported file in this dedicated folder, click again, and confirm
   the packaged content was restored. Repeat once without editing. Confirm no
   prompt, unexpected game/save action or plugin exception.
4. Hide/show with F8; confirm import is never automatic and the guide link still
   opens WHITE. Check placement at the display/UI scale normally used.
5. Set `ExpertMode = false`, restart, and confirm the button is absent and normal
   navigation works. Repeat the Expert-mode import/normal-mode absence check
   with the other DLL variant; never install both together.

**Out of scope:** Blueprint generation/repair, automatic placement, gameplay
validation of the pack, runtime mode switching, new guide phases, version
promotion, publication, unrelated cleanup or deleting other blueprint files.

## Validation and handoff

Both Release variants built with zero warnings/errors. `Test-BlueprintImport.ps1`
passed on the diagnostic DLL and through `Test-Guide3Portable.ps1` on the public
DLL: all 219 files matched exactly; every unchanged file was rewritten; same-size
edits were replaced; neighboring/unrelated files stayed intact; missing/relative
paths and redirected folders were rejected; a locked-file failure recovered on
the next click. One test-fixture repair handled PowerShell's exception wrapper.

`Test-ExpertConfiguration.ps1` passed on both DLLs, including default/false/true
config, normal-mode callback guards, two Expert callback invocations, synthetic
native-path resolution and missing-path soft failure, selection preservation and
no hidden-refresh import. No actual player blueprint directory was touched by
automated tests. `Test-SnapshotControlVariant.ps1` passed for both variants.
`New-ThunderstorePackage.ps1` and `Test-ThunderstorePackage.ps1` passed for the
public test package, including its embedded DLL hash and unchanged ZIP layout.

The owner reported that validations passed per this story on 2026-10-04,
accepting the actual Unity placement, import and overwrite behavior, native
blueprint discovery, and Expert-only visibility in both variants. This is
owner-reported runtime evidence, separate from the automated checks above.

The accepted builds were local `3.1.0` artifacts under
`artifacts/expert-blueprint-import/`; their `HANDOFF.json` records their hashes.
The owner then authorized committing and pushing the feature, checking
publication readiness and promoting the minor version. Publication is a
separate release step tracked by the [current project state](../../PROJECT.md).
