# Beat Saber Server Browser (PC)
⚔ **A mod for Beat Saber that adds a Server Browser to Online, making it easy to share and join multiplayer games.**

|**⏬ Download**|**[Latest release](https://github.com/roydejong/BeatSaberServerBrowser/releases/latest)**|
|---|---|
|**🌎 Online**|**[https://bssb.app](https://bssb.app)**|

## Overview
Use this mod to add your games to the server browser, and to browse and join multiplayer matches from within the game. Only games hosted with this mod can be seen in the browser.

### Features
- **Server browser**: Browse and join servers directly from the online menu.
- **Host settings**: Add your server to the browser with a single switch to make it public, and set a custom name.
- **Custom games**: Supports modded / custom song lobbies with [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions) (*Early access*).
- **Cross-play**: Automatic switching between official and modded master servers, like [BeatTogether](https://discord.com/invite/gezGrFG4tz) (*Early access*).
- **Join/leave notifications**: An optional feature that lets you know when players join or leave, even in a level.

<p align="center">
    <img src="https://user-images.githubusercontent.com/6772638/105616590-80ca6900-5dd8-11eb-9f76-9785b05cb524.png" alt="Beat Saber Server Browser">
</p>

### Joining games

<img src="https://user-images.githubusercontent.com/6772638/105616739-62b13880-5dd9-11eb-9d67-86da191af753.png" alt="Online Menu" align="right" width="362">

After installing the mod, you can find the Server Browser in the Online menu. From there, simply select a game and then click Connect to join up.

If the game is hosted on a third party cross-play server, we'll switch your master server automatically, even if you don't have a cross-play mod installed.

To join lobbies with custom songs, you will need MultiplayerExtensions; you will only see compatible games.

### For hosts

<img src="https://user-images.githubusercontent.com/6772638/105617023-da806280-5ddb-11eb-9891-a8c7ac7c1264.png" alt="Create Server" align="right" width="316">

When creating a new server, you will see the option to add your game to the server browser. You will also be able to set a custom name here if you want.

From the lobby, you can also change your server browser settings from the panel under *Gameplay Setup*. You'll be able to add or remove your game at any time, change the name, and toggle notifications here.

While your game is added to the browser, anyone will be able to join and your server code will be made public on the site (https://bssb.app).

## Installation
The latest version of the mod (v0.4) is not yet available on Mod Assistant, but will be soon.

**You can download the latest build from the [Releases](https://github.com/roydejong/BeatSaberServerBrowser/releases) page, and extract it to your Beat Saber folder.**

*If the mod was successfully installed, you will see `ServerBrowser.dll` in your `Plugins` directory.*

### Requirements
These libraries are required, install them [from BeatMods](https://beatmods.com/#/mods) or [with ModAssistant](https://github.com/Assistant/ModAssistant) if you don't have them yet:

- BSIPA v4.1.4+
- BeatSaberMarkupLanguage: v1.4.5+
- SongCore: v3.0.3+
- BS Utils: v1.7.0+

### Recommended
I **highly** recommend installing [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions), which lets you play custom levels in multiplayer, and adds some useful new features.

Please note MultiplayerExtensions is still in active development and things may break or change at any time.

## FAQs / Troubleshooting

### Is standalone Quest supported?
The mod itself is only available for PC at this time (Steam and Oculus).

Standalone Quest is not currently supported, but you may be able to find games to join in the online browser: [https://bssb.app](https://bssb.app).

### Does cross-play work?
Yes, but this depends on the host. To host a cross-play game, the host must use a cross-play mod like **[BeatTogether](https://discord.com/invite/gezGrFG4tz)**. In that case, anyone can see and join the game.

However, if a game is hosted on *official* Steam or Oculus servers, cross-play will not work. You will not see incompatible games on other platforms (for example, Steam users will not see games hosted on official Oculus servers).

### Are Quick Play servers supported?
No.
