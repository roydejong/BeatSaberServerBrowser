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
- **Custom games**: Supports modded / custom song lobbies with [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions).
- **Cross-play**: Automatic switching between official and modded master servers, like [BeatTogether](https://discord.com/invite/gezGrFG4tz).
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
👉 **The easiest way to install this mod is through [ModAssistant](https://github.com/Assistant/ModAssistant)! The latest version of the mod will be available there soon.**

### Manual installation
You can manually download the latest version of the mod from the [Releases](https://github.com/roydejong/BeatSaberServerBrowser/releases/latest) page. From there, download the ServerBrowser zip file and extract it to your Beat Saber folder.

*If the mod was successfully installed, you will see `ServerBrowser.dll` in your `Plugins` directory.*

### Requirements
These libraries are required, install them [from BeatMods](https://beatmods.com/#/mods) or [with ModAssistant](https://github.com/Assistant/ModAssistant) if you don't have them yet. If you're using ModAssistant, these are installed automatically.

- BSIPA v4.1.6+ *(Core mod)*
- SongCore: v3.4.0+ *(Core mod)*
- BeatSaberMarkupLanguage: v1.5.3+

Optional requirement:

- DiscordCore: v1.0.5+ *(for Rich Presence support)*

### MultiplayerExtensions
**I recommend installing [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions), which lets you play custom levels in multiplayer and adds some useful new features.** You can get the latest version through GitHub, or install it through ModAssistant.

Please note, you and the host must use the same version of MultiplayerExtensions. If a version difference is detected by the server browser, you will not be able to connect.

## Rich Presence (BETA)
The Server Browser mod supports Rich Presence for Discord. This feature lets you join and invite players directly through Discord!

### How to use Discord presence
- You must have the DiscordCore library installed (available on ModAssistant).
- You must have Discord running on the same PC as Beat Saber.

### How to send invites
If you are hosting a game, or if you have joined a public game, you can invite users directly through Discord by clicking the ➕ button:

![Discord invite](https://user-images.githubusercontent.com/6772638/124370610-e705c280-dc79-11eb-98a2-4225b4b783cd.png)

Invites expire after 2 hours, or when the host ends the game.

### How to use invites
**⚠ To accept invites, you must be in the Online menu in Beat Saber.**


## FAQs / Troubleshooting

### Does cross-play work?
**That depends on the host!** To host a cross-play game, the host must use a cross-play mod like **[BeatTogether](https://discord.com/invite/gezGrFG4tz)**. In that case, anyone can see and join the game through the server browser regardless of platform.

However, if a game is hosted on *official* Steam or Oculus servers, cross-play will not work. You will not see incompatible games on other platforms (for example, Steam users will not see games hosted on official Oculus servers).

### Is standalone Quest supported?
The mod itself is only available for PC at this time (Steam and Oculus). Standalone Quest is not currently supported. 

However: you may be able to find Quest-compatible games to join in the online browser: **[https://bssb.app](https://bssb.app)**:
 - If your game is not modded, you can connect to games hosted on **Oculus (Official)** servers (no custom song support).
 - If you have BeatTogether installed, you can connect to games hosted on **Unofficial** servers like BeatTogether.

### Are Quick Play servers supported?
Not *yet*. Coming soon!

### Do players need to be on the same game version?
Yes. Different versions of Beat Saber are not compatible with each other for multiplayer.

The Server Browser will only show games hosted on the exact same version of the game as yours.
