# DSP Guide Check

DSP Guide Check is an on-demand progression companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It reads the current save, evaluates the guide phase selected by the player,
and presents stable objectives plus concise, phase-aware status.

The player asks; the instrument answers. The panel is hidden by default,
never changes the factory or save, and never advances phases automatically.

## Features

- Manual phase navigation through the nine critical-path phases from BLUE
  through WHITE.
- Stable objectives based on the
  [DSP Practical Progression Guide](https://dsp-beginner-guide.pages.dev/).
- Native Statistics Panel, logistics, power, Dyson, and Ray Receiver evidence.
- Player-requested compact JSON snapshots for diagnostics.
- A native-styled, collapsible, scrollable, and click-through panel.
- Embedded Basic Regular presentation font with a high-contrast outline.

## Installation

Install with a Thunderstore-compatible mod manager, or copy the included
`BepInEx` directory into the Dyson Sphere Program game directory.

[BepInEx 5](https://thunderstore.io/c/dyson-sphere-program/p/xiaoye97/BepInEx/)
is required.

## Use

Press **F8** after loading a save.

- F8 opens or closes the panel.
- Previous and next move between phases.
- `Save snapshot` writes one compact JSON file on demand.
- `DON'T PANIC` opens the source guide at the selected phase.

The selected phase belongs to the player. Runtime evidence evaluates that
selection but never advances or regresses it automatically.

## Links

- [Source repository](https://github.com/shytamir/dsp-beginner-guide)
- [Practical progression guide](https://dsp-beginner-guide.pages.dev/)
- [Issue tracker](https://github.com/shytamir/dsp-beginner-guide/issues)
- [Apache License 2.0](https://github.com/shytamir/dsp-beginner-guide/blob/main/LICENSE)

This is an unofficial community project. Dyson Sphere Program and its assets
belong to their respective owners.
