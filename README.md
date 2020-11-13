# Beat Saber Server Browser
✨ **A Beat Saber modification that adds a server browser to multiplayer!**

|**⏬ Download**|**[Latest release](https://github.com/roydejong/BeatSaberServerBrowser/releases/latest)**|
|---|---|
|**🌎 Online**|**[https://bssb.app](https://bssb.app)**|

## Overview
Use this mod to find and join servers without ever having to leave the game.

👉 The host must have this mod installed and enabled to add their server to the browser.

### Features
- **Ingame browser**: Browse and join servers directly from the multiplayer menu.
- **Host panel**: Use the lobby panel to add your server to the browser and set a public name.
- **Custom games**: Supports modded / custom song lobbies if you have [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions) (*Early access*).
- **Cross-play**: We'll help you join games on both official and unofficial servers (like [BeatTogether](https://discord.com/invite/gezGrFG4tz)) by switching your master server automatically. (To host a cross-play game, you'll need to install a cross-play mod.)

![Server Browser Mod](https://user-images.githubusercontent.com/6772638/97793891-9cb3d980-1bf2-11eb-8785-ed99c11f6db6.png)

### Joining games
After installing the mod, you can find the Server Browser in the Multiplayer menu.

From there, simply select a game and click Connect to join up.

If the game is hosted on a third party cross-play server, we'll switch your master server automatically, even if you don't have a cross-play mod installed.

## For hosts
To show your game in the server browser, you must have the mod installed and enabled. 

You can then make your game visible by flipping the switch under Gameplay Setup. This will let anyone join from the browser:

![Host UI](https://user-images.githubusercontent.com/6772638/97793912-f7e5cc00-1bf2-11eb-9302-0d9292a64288.png)

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
