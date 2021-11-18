# Beat Saber Server Browser (PC)
⚔ **A mod for Beat Saber that adds a Server Browser to Online, making it easy to share and join multiplayer games.**

<table>
    <tr>
        <td><strong>⏬ PC Download</strong></td>
        <td><strong><a href="https://github.com/roydejong/BeatSaberServerBrowser/releases/latest" target="_self">Latest release</a></strong></td>
    </tr>
    <tr>
        <td><strong>🆕 Quest mod</strong></td>
        <td><strong><a href="https://github.com/EnderdracheLP/BeatSaberServerBrowserQuest/releases/latest" target="_self">BeatSaberServerBrowserQuest</a></strong></td>
    </tr>
    <tr>
        <td><strong>🌎 Online</strong></td>
        <td>https://bssb.app</td>
    </tr>
</table>

## Overview
Use this mod to add your games to the server browser, and to browse and join multiplayer matches from within the game. Only games hosted with this mod can be seen in the browser.

### Features
- **Server browser**: Browse and join servers directly from the online menu.
- **Host settings**: Share your lobbies to the server browser with a custom name, and control the settings directly from the lobby.
- **Custom games**: Supports modded / custom song lobbies with [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions).
- **Master server switching**: Automatic switching between official and modded master servers, like [BeatTogether](https://discord.com/invite/gezGrFG4tz).
- **Join/leave notifications**: An optional feature that lets you know when players join or leave, even in a level.
- **Rich presence**: Adds rich presence support for Discord and Steam invites.

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
These mods are required, install them [with ModAssistant](https://github.com/Assistant/ModAssistant) if you don't have them yet:

- BSIPA *(Core mod)*
- SongCore *(Core mod)*
- BeatSaberMarkupLanguage

Optional requirement:

- DiscordCore *(if you want Rich Presence / Discord invites)*

### MultiplayerExtensions
**I recommend installing [MultiplayerExtensions](https://github.com/Zingabopp/MultiplayerExtensions), which lets you play custom levels in multiplayer and adds some useful new features.** You can get the latest version through GitHub, or install it through ModAssistant.

Please note, you and the host must use the same version of MultiplayerExtensions. If a version difference is detected by the server browser, you will not be able to connect.

## Rich Presence (BETA)
The server browser mod adds rich presence support for Steam and Discord. This lets you join and invite players directly through these platforms!

### Requirements
Platform specific:
- Discord Rich Presence requires the DiscordCore mod to be installed, and Discord must be running.
- Steam Rich Presence is automatically enabled if you have the Steam version of the game.

In general:
- 👉 The multiplayer game must be publicly shared on the server browser.
- 👉 To accept invites, you must already be in the Online menu in Beat Saber.
- Both players must have the server browser mod.
- Both players should be on the same game version and MultiplayerExtensions version. 

### Inviting and joining players
Once you're in a public server browser lobby, other players can join you, and you can send invites.

#### On Discord
You can send an invite to any channel or DM via the plus botton:

![Discord invite send](https://user-images.githubusercontent.com/6772638/133778816-ed0bcdc1-095f-44af-9cee-2165081496ca.png)

Other players can also "Ask to Join" you from your Discord status.

- 🕑 Discord invites are slow! Please be patient. It can take minute before you can send or accept an invite. 
- ⏰ Invites expire after 2 hours, or whenever the host ends the game.

#### On Steam
Right click a friend to send an invite, or to join their game.

![Steam invites](https://user-images.githubusercontent.com/6772638/140385776-6ddced18-4360-4a1c-86cf-8d3269fb7f1a.png)

You can also click "View game info" to see a friend's extended multiplayer status if they are in a lobby:

![Steam game info](https://user-images.githubusercontent.com/6772638/140385211-e321ea00-1181-4128-9913-c0ef95ebec8e.png)

## FAQs / Troubleshooting

### How do I play custom songs?
You need to have MultiplayerExtensions installed to play custom songs.

If you are the host, you must also have BeatTogether installed, and have BeatTogether selected as your server.

Custom songs do NOT work on Official Servers.

### Does cross-play work?
🎉 Yes! Beat Saber has enabled cross-play for all platforms on official servers.

Unofficial servers like BeatTogether always allow cross play.

The Server Browser will now show all games regardless of the host's platform.

### Is standalone Quest supported?
EnderdracheLP has released a Lite[^questlite] version of the mod for Quest, check it out here:

[https://github.com/EnderdracheLP/BeatSaberServerBrowserQuest](https://github.com/EnderdracheLP/BeatSaberServerBrowserQuest)

[^questlite]: The lite version of BeatSaberServerBrowserQuest only lets you join games that were announced via the PC mod.

### Are Quick Play servers supported?
Yes! You can now share and join Quick Play servers through the Server Browser. 

The games are only visible if someone in the lobby is announcing it to the Server Browser - Quick Play lobby data is crowd sourced!

### Do players need to be on the same game version?
Generally speaking: Yes. Different versions of Beat Saber are usually not compatible with each other for multiplayer.

The Server Browser will only show games hosted on the exact same version of the game as yours, or game versions that are known to be compatible[^compatvers].

[^compatvers]: The server browser currently treats 1.17 and 1.18 game versions as compatible, to make it easier for quest and PC users to play together.
