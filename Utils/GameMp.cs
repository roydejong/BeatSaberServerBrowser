using System.Linq;
using UnityEngine;

namespace LobbyBrowserMod.Utils
{
    public static class GameMp
    {
        public static MultiplayerSessionManager SessionManager
        {
            get => Resources.FindObjectsOfTypeAll<MultiplayerSessionManager>().First();
        }
    }
}
