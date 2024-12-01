# TeamDeathMatch
Fight against eachother in this classic gamemode, now in Blade and Sorcery!

## Features
- Automatic team building
- Team indicators above players
- Configurable round and intermission timer
- Score counters

## How to use
This is a plugin for [Adammantium's Multiplayer Mod](https://www.nexusmods.com/bladeandsorcery/mods/6888). To use it, you will need to set up your own [dedicated server](https://github.com/AdammantiumMultiplayer/Server). Once that's set up just drag and drop the `TeamDeathMatch.dll` file into the `plugins` folder!

## Config
The config is automatically generated after the initial run of the plugin. It is located in the `plugins` folder and is called `TeamDeathMatch.json`.
| Tag                   | Description                                              | Value Type   | Default Value   |
|-----------------------|----------------------------------------------------------|--------------|-----------------|
| `requiredPlayerCount` | The minimun number of players required to start a match. | `int`        | `2`             |
| `matchTime`           | How long the match will last. In seconds.                | `float`      | `300.0`         |
| `intermissionTime`    | How long between matches. In seconds.                    | `float`      | `10.0`          |
