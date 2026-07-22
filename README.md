# SSSS

###### Why the hell there's no normal save in this game???

# Description

_Stoneshard Save Scumming_, or simply _SSSS_, is a directory monitoring service that automatically backs up `exitsave_1` directory for a ~~given~~ `#4` character and restores them the moment the game deletes them after the save is loaded.

It watches the exit save directory and creates backups (up to `10`) whenever it detects changes, allowing you to reload the previous state of your game.

# Usage

- Build the solution.
- Ensure `\AppData\Local\StoneShard\characters_v1\character_4` exists.
- Run the `.exe` file.

# Backups

Backups are stored in the `backups` directory, which is created in the same directory as the executable. Replace the existing `exitsave_1` directory with any of those (keeping the `exitsave_1` name) to be able to load that save file in game.

# Roadmap

- Configurable character selection
- Browser management page