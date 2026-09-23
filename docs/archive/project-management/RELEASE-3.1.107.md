# DSP Guide Check 3.1 compatibility and release record

**Completed:** 2026-09-24. The owner confirmed tag `3.1` and successful
Thunderstore publication of **3.1.107**. Compatibility documentation,
minor-version promotion and artifact verification are complete. The repository
is in maintenance mode with no active epic, story or acceptance gate.

**Current authority:** [PROJECT.md](../../PROJECT.md). This completed record
does not authorize further implementation.

## Completed work

- On 2026-09-23, the owner reported that all existing validations passed after
  the update to DSP Early Access `0.10.35.29057`, with no runtime changes needed.
- Commit `ccc1032` updated the current compatibility statement. The documentation
  scan retained historical game versions and assembly hashes as evidence of the
  binaries originally inspected.
- Commit `1a56195cf09cb227763a369fc9fde4057f10e43d` promoted the minor version
  from 3.0 to 3.1 in `VERSION`, local plugin/exporter constants and project
  metadata, with corresponding documentation. Behavior and snapshot contracts
  remained unchanged. No implementation roadmap was needed.
- On 2026-09-24, the owner confirmed store publication. The remote `3.1` tag
  was independently checked and pointed to that promotion commit.

## Release identity and validation

[GitHub Actions run 107](https://github.com/shytamir/dsp-beginner-guide/actions/runs/35921824373)
completed successfully for the promotion commit. Its downloaded artifact was
independently verified before publication confirmation.

| Identity | Verified value |
|---|---|
| Package, BepInEx plugin and exporter | `3.1.107` |
| Assembly and file metadata | `3.1.107.0` |
| Diagnostic label | `3.1.107.1a56195` |
| Git tag | `3.1` |

Validation performed during promotion:

- Release `dotnet build`: zero warnings and errors after retrying with access
  to the installed SDK; the initial sandbox attempt could not read its path.
- `Test-Guide3Candidate.ps1 -Sequence 107`: clean-source verification passed
  for both variants, behavior fixtures, compiled identities, snapshot parity
  and the installable package against the installed game references.
- Downloaded Actions archive: SHA-256 matched GitHub's artifact digest.
- Downloaded DLLs: compiled BepInEx attributes, exporter constants and release
  labels were checked independently. `Test-BuildArtifact.ps1` and
  `Test-SnapshotControlVariant.ps1` passed for both variants.
- `Test-ThunderstorePackage.ps1`: package name, layout, manifest and embedded
  public-DLL hash passed. Downloaded DLL/package hashes matched the hosted
  reports. Snapshot control was present only in the diagnostic DLL.

| Verified CI artifact | SHA-256 |
|---|---|
| Public ZIP | `fba3b264dc6bbbb8b5d201d5c565d39329ffd5d28495bfd51908dcd6b50d17a6` |
| Public DLL | `d548904d2797034018627d8a182361514c4e9300ec4bfc8e6044932a0cdaf2d4` |
| Diagnostic DLL | `f23976e498895545e3ec01836657c430edef3ccebec3c15165cccfc0086ec6e8` |

The game-validation and store-publication confirmations came from the owner.
The promotion checks did not add a new in-game playthrough. Later CI builds do
not replace this published baseline without a separate release decision.
