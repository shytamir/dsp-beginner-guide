# DSP Guide Check

DSP Guide Check is an on-demand progression companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It implements the [DSP Practical Progression Guide 3.0](https://dsp-beginner-guide.pages.dev/)
default critical path, with stable objectives and concise advice for the phase
you choose.

The panel starts hidden. It never changes your factory, save or selected phase,
and it shows no unsolicited alerts.

## Features

- Nine player-selected phases from BLUE through WHITE.
- ILS Departure, Haulback and Automation stages saved for each playthrough.
- Stable objectives, live Cube production counters and concise shortage advice.
- A collapsible, scrollable panel styled to match the game.
- `DON'T PANIC` opens the guide at the selected phase or ILS stage.
- Optional [Expert mode](#expert-mode): set `ExpertMode = true` in the config
  to show `Import Blueprints`, all six Cube counters and `DON'T PANIC` without
  the guidance panel.
- Read-only game/save observation with no automatic phase changes or factory actions.

## Requirements and installation

Dyson Sphere Program and BepInEx 5 are required. DSP Guide Check **3.1.107**
fully supports DSP Early Access `0.10.35.29057`. The owner also accepted the
new Expert-only blueprint import through its runtime validation checklist.

Use a Thunderstore-compatible mod manager to install DSP Guide Check and
launch DSP with mods enabled. For manual installation, install BepInEx 5
and copy the package's `BepInEx` folder into the game directory.

## Use

Load a save and press **F8**:

- F8 opens or closes the overlay.
- Previous and Next select the guide phase.
- In ILS, choose Departure, Haulback or Automation.
- Collapse or scroll the panel when you want more room.
- Select `DON'T PANIC` to read the matching guide section.

Your phase and ILS stage are retained for the playthrough across saves and
restarts. The first phase suggestion uses the latest researched Cube; you
can change it at any time.

```text
BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
     -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM are checkpoints inside ILS. Optional routes remain in
the guide. The mod does not provide combat advice or a post-game
dashboard.

### Expert mode

In the mod's BepInEx config file, set:

```ini
[General]
ExpertMode = true
```

The default is `false`. Restart DSP after changing it.

Expert mode shows **`Import Blueprints`**, **all six Cube counters** and
**`DON'T PANIC`** when you press F8. It uses WHITE for the counters and guide link. The adjoining
panel and its controls are hidden, including Previous/Next, collapse and
ILS stage selection.

`Import Blueprints`, above the Blue Cube counter, unpacks the bundled guide
playthrough collection into `Guide Check` inside DSP's configured blueprint
folder. Each click writes the complete pack again, silently overwriting matching
files while retaining its folder structure. There is no automatic import;
unrelated files remain. Import failures are recorded in the plugin log.
The button is available only with `ExpertMode = true`.

After importing, open or reopen DSP's blueprint browser and select `Guide Check`
to browse and use the collection. Copy any blueprint you want to customize to
another folder first: importing again restores the bundled files.

Set `ExpertMode = false` and restart to restore the guidance panel and your
saved phase and ILS stage. F8 never saves in either mode.

## Support and development

For a bug report, include the DSP/mod versions, selected phase, expected
behavior and a screenshot where useful. See [CONTRIBUTING.md](CONTRIBUTING.md)
for reporting, source builds and diagnostic guidance.

The published version is **3.1.107**, tagged `3.1`. The
[Expert-only import](docs/archive/project-management/EXPERT-BLUEPRINT-IMPORT-01.md)
was accepted by the owner on 2026-10-04 and is being prepared for publication;
it is not part of that published baseline. See
[project state](docs/PROJECT.md), [current work](docs/ROADMAP.md) and
[release history](CHANGELOG.md).

## License

DSP Guide Check is licensed under the [Apache License 2.0](LICENSE).

This is an unofficial community project. Dyson Sphere Program and its assets
belong to their respective owners. BepInEx and the game are required but are
not included.
