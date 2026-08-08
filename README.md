# SSSS

###### Why the hell there's no normal save in this game???

# Description

_Stoneshard Save Scumming_, or simply _SSSS_, is a directory monitoring service that automatically backs up `exitsave_1` directory for a given character and restores it the moment the game deletes it after the save is loaded.

It watches the exit save directory and creates backups (up to `10`) whenever it detects changes, allowing you to reload the previous state of your game.

# Usage

- Download the latest release or build the app yourself from the source code.
- Configure the character to monitor via `appsettings.json`.
- Run the `.exe` file.

Updating `appsettings.json` while the program is running will automatically start monitoring of the specified character.

# Backups

Backups are stored in the `backups` directory, which is created in the same directory as the executable. Replace the existing `exitsave_1` directory with any backup you have (keeping the `exitsave_1` name) to be able to load that save file in game.

# Roadmap

- [x] Configurable character selection
- [ ] Configurable backups count
- [ ] Split backups for each character (?)
- [ ] Browser management page