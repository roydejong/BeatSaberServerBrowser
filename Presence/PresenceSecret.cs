using System.Text;
using Newtonsoft.Json;
using ServerBrowser.Core;
using ServerBrowser.Game;
using ServerBrowser.Utils.Serialization;

namespace ServerBrowser.Presence
{
    public struct PresenceSecret
    {
        public enum PresenceSecretType : byte
        {
            Match = 0,
            Join = 1,
            Spectate = 2
        }
        
        [JsonProperty("t")]
        public PresenceSecretType SecretType;
        
        [JsonProperty("me")]
        [JsonConverter(typeof(MasterServerEndPointJsonConverter))]
        public MasterServerEndPoint MasterServerEndPoint;
        
        [JsonProperty("sc")]
        public string ServerCode;
        
        [JsonProperty("hs")]
        public string? HostSecret;

        [JsonProperty("st")]
        public string ServerType;

        public void Connect()
        {
            // Check connection state
            var mpActivity = GameStateManager.Activity;

            if (!mpActivity.InOnlineMenu || mpActivity.IsInMultiplayer)
            {
                Plugin.Log?.Error("[Presence] Ignoring Rich Presence invite: must be in online menu!");
                return;
            }
            
            Plugin.Log?.Info("[Presence] Connecting to session via Rich Presence invite" +
                             $" (MasterServerEndPoint={MasterServerEndPoint}, ServerCode={ServerCode}" +
                             $", HostSecret={HostSecret}, ServerType={ServerType})");
            
            // Set master server
            MpConnect.SetMasterServerOverride(MasterServerEndPoint);
            
            // Try to join
            MpConnect.Join(new()
            {
                MasterServerHost = MasterServerEndPoint.hostName,
                MasterServerPort = MasterServerEndPoint.port,
                ServerCode = ServerCode,
                HostSecret = HostSecret,
                ServerType = ServerType,
                GameName = "Game Invite"
            });
        }
        
        public override string ToString()
        {
            var str = JsonConvert.SerializeObject(this, Formatting.None);
            Plugin.Log.Debug($"Secret: {str} ({str.Length} len / {Encoding.UTF8.GetByteCount(str)} bytes)");
            return str;
        }
        
        public static PresenceSecret FromString(string input)
        {
            return JsonConvert.DeserializeObject<PresenceSecret>(input);
        }
    }
}