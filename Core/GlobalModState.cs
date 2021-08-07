using System.Net;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Global connection modifiers used by Server Browser Harmony patches.
    /// </summary>
    public static class GlobalModState
    {
        /// <summary>
        /// If true, the server browser initiated the current connection.
        /// This indicates that connection checks and patches may be enabled.
        /// </summary>
        public static bool WeInitiatedConnection  = false;
        
        /// <summary>
        /// If true, the server browser aborted a join it had previously initiated.
        /// This typically happens when a dedicated server instance is no longer available.
        /// This value is used to present the appropriate error message to the user.
        /// </summary>
        public static bool WeAbortedJoin = false;
        
        /// <summary>
        /// The last game the server browser attempted to connect to.
        /// </summary>
        public static HostedGameData? LastConnectToHostedGame = null;

        /// <summary>
        /// If set, performs a direct server connection to the target IPEndPoint.
        /// This will effectively ignore and override what the master server tells the client.
        /// Important: This ONLY works for servers that explicitly support this (BeatDedi). 
        /// </summary>
        public static IPEndPoint? DirectConnectTarget = null;

        /// <summary>
        /// Indicates whether encryption should be disabled on connections.
        /// This is used to enable direct connections to servers that support it, as there is no encryption handshake.
        /// </summary>
        public static bool ShouldDisableEncryption = false;

        /// <summary>
        /// Resets the mod state, disabling any special networking patches and behaviors.
        /// </summary>
        public static void Reset()
        {
            WeInitiatedConnection = false;
            WeAbortedJoin = false;
            LastConnectToHostedGame = null;
            DirectConnectTarget = null;
            ShouldDisableEncryption = false;
        }
    }
}