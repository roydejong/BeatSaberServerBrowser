# Beat Saber Server Browser (PC)
**A mod for Beat Saber that adds a Server Browser to the Online menu, making it easy to share and join custom multiplayer games.**

|**⏬ Download**|**[Latest release](https://github.com/roydejong/BeatSaberServerBrowser/releases/latest)**|
|---|---|
|**🌎 Online**|**[https://bssb.app](https://bssb.app)**|

## Overview
Install this mod to add your games to the server browser, and browse and join multiplayer matches from within the game.

### Features
- **Server browser**: Browse and join servers directly from the online menu.
- **Host settings**: Add your server to the browser with a single switch to make it public, and set a custom name.
- **Custom games**: Supports modded / custom song lobbies with [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions) (*Early access*).
- **Cross-play**: Automatic switching between official and modded master servers, like [BeatTogether](https://discord.com/invite/gezGrFG4tz) (*Early access*).
- **Join/leave notifications**: Optional notifications that let you know when players join or leave, even in a level.

<p align="center">
    <img src="https://user-images.githubusercontent.com/6772638/105616590-80ca6900-5dd8-11eb-9f76-9785b05cb524.png" alt="Beat Saber Server Browser">
</p>

### Joining games

<img src="https://user-images.githubusercontent.com/6772638/105616739-62b13880-5dd9-11eb-9d67-86da191af753.png" align="right" width="362">

After installing the mod, you can find the Server Browser in the Online menu.

From there, simply select a game and then click Connect to join up.

If the game is hosted on a third party cross-play server, we'll switch your master server automatically, even if you don't have a cross-play mod installed.

To join lobbies with custom songs, you will need MultiplayerExtensions. You will only see games that are compatible with your setup.

<img src="https://user-images.githubusercontent.com/6772638/105616739-62b13880-5dd9-11eb-9d67-86da191af753.png" align="left" width="362">

### For hosts
When creating a new server, you will see the option to add your game to the server browser. You will also be able to set a custom name here.



## Installation
Download the latest build from the [Releases](https://github.com/roydejong/BeatSaberServerBrowser/releases) page, and extract it to your Beat Saber folder.

*If the mod was successfully installed, you will see `ServerBrowser.dll` in your `Plugins` directory.*

### Requirements
These libraries are required, install them [from BeatMods](https://beatmods.com/#/mods) or [with ModAssistant](https://github.com/Assistant/ModAssistant) if you don't have them yet:

- BSIPA: v4.1.3+
- BeatSaberMarkupLanguage: v1.4.0+
- SongCore: v3.0.0+
- BS Utils: v1.6.2+

### Recommended
I **highly** recommend installing [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions), which lets you play custom maps in multiplayer! You can see which games are "modded" in the browser.

Please note MultiplayerExtensions is still in development and things may break or change. Use it at your own risk.

## FAQs / Troubleshooting

### Is standalone Quest supported?
The mod itself is only available for PC at this time (Steam and Oculus).

Standalone Quest is not currently supported, but you may be able to find games to join in the online browser: [https://bssb.app](https://bssb.app).

### Does cross-play work?
Maybe. It depends on who is hosting the game:

- If the game is hosted on official Steam or Oculus servers, cross-play does NOT work. The game will be hidden from users on other platforms.
- If the game is hosted with a cross-play mod (like [BeatTogether](https://discord.com/invite/gezGrFG4tz)), cross play DOES work. Anyone can see and join the game.

Note: This mod detects your platform as either Steam or Oculus, and will hide games on incompatible official servers.

### Are Quick Play servers supported?
No.
