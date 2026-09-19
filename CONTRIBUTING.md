# Contributing

The project is in maintenance mode. Bug reports and focused improvements are
welcome; no implementation work is active. Read [AGENTS.md](AGENTS.md) and
[the product contract](docs/PROJECT.md) before changing the code.

## Bug reports

Please include:

- DSP and DSP Guide Check versions;
- selected phase and ILS stage, and whether Expert mode is enabled;
- expected and observed behavior;
- a screenshot for presentation or wording issues;
- whether the problem repeats after reloading the save.

The public package has no snapshot control. When runtime evidence is needed,
use the diagnostic DLL and its deliberate `Save snapshot` action. Do not install
both DLL variants together. Snapshots can contain planet names and save details;
review them before sharing. F8 never exports a snapshot.

## Changes

Proposed work should identify the published-guide change, reproducible defect,
compatibility issue or in-scope player need it addresses. Preserve player-owned
selection, stable objectives, concise status, soft failure for missing evidence,
and passive read-only operation.

Run the narrowest relevant deterministic checks and build against installed
DSP references. Use the affected cases in
[the maintenance regression protocol](docs/RUNTIME-TESTING.md) for runtime or
presentation changes. Compilation does not prove in-game behavior.

## Build

Run `build.cmd` from the repository root. By default it uses the standard
Steam installation:

```text
C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program
```

To use another game directory:

```text
build.cmd "D:\Games\Dyson Sphere Program"
```

The DLL is written to:

```text
src\DspProgressionStatusExporter\bin\Release\net472\DspGuideCheck.dll
```

The game and BepInEx assemblies are referenced from the local installation;
they are not redistributed in this repository.

### Verify both build variants

From a clean checkout, run with PowerShell 7:

```powershell
pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1
```

Use `-GameRoot` for another DSP installation and `-Sequence N` to match a
GitHub workflow run number. The default sequence 1 is local verification only;
CI keeps using its run number as the patch. Output under ignored
`artifacts/guide3/candidate-<source>-<sequence>/` includes both DLLs, public ZIP,
source archive, versions/hashes, test logs, snapshot fixtures and a copy of the
archived Guide 3.0 workshop record. That record describes historical owner
evidence, not acceptance of a later build.
The command verifies installed game references and uses the existing local SDK.
PowerShell 7 is required for the local hidden-controller fixture; Windows
PowerShell 5 cannot resolve its Unity calls.
No game session or player save is needed. `-AllowWorkingTree` is a preflight
option; its output does not identify a clean, committed release source.

## Versioning and continuous integration

`VERSION` records the manually selected major and minor version numbers. Every
push to `main` runs the build, DLL verification, Thunderstore packaging, and
package verification workflow. The workflow sequence becomes the patch
number:

```text
Package/plugin version: M.m.N
Assembly/file version:  M.m.N.0
Diagnostic label:       M.m.N.X
```

For example, workflow run 42 produces package and BepInEx version `3.0.42`,
assembly/file version `3.0.42.0`, and diagnostic label
`3.0.42.abcdef1`. The workflow sequence advances without committing a
generated version change back to `main`.

The hosted build downloads the official BepInEx 5 release as a compile
reference and restores pinned Unity reference packages. Local release builds
continue to use the assemblies supplied by the installed game and remain the
authoritative compatibility check.

BepInEx and the Thunderstore manifest receive the same three-number version.
Snapshots and reports retain the commit-bearing diagnostic label. The build
test rejects an invalid BepInEx identity, and the package test rejects an
incorrect manifest, icon, README, file name, or ZIP layout.

The exact deployment contract is documented in
[docs/THUNDERSTORE-PACKAGE.md](docs/THUNDERSTORE-PACKAGE.md).

## Runtime evidence and packaging

[Native telemetry reference](docs/NATIVE-TELEMETRY-ALIGNMENT.md) documents
authoritative game fields and evidence scope. [The package contract](docs/THUNDERSTORE-PACKAGE.md)
documents the public/diagnostic split and release artifact checks.

Later CI artifacts do not supersede a published release without a separate
release decision. The published baseline is recorded in
[PROJECT.md](docs/PROJECT.md). Completed implementation and acceptance records
are kept in [the archive](docs/archive/README.md).
