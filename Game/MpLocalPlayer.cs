using System.Linq;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;

namespace ServerBrowser.Game
{
    public static class MpLocalPlayer
    {
        private static LocalNetworkPlayerModel? _localNetworkPlayerModel;

        public static UserInfo? UserInfo
        {
            get;
            private set;
        }

        public static UserInfo.Platform? Platform => UserInfo?.platform;

        public static string PlatformId
        {
            get
            {
                switch (Platform)
                {
                    case UserInfo.Platform.Oculus:
                        return "oculus";
                    case UserInfo.Platform.Steam:
                        return "steam";
                    case UserInfo.Platform.PS4:
                        return "ps4";
                    case UserInfo.Platform.Test:
                        return "test";
                    default:
                    case null:
                        return "unknown";
                }
            }
        }

        public static async Task SetUp()
        {
            // Note: The game creates one local player in MainSystemInit.InstallBindings()
            
            _localNetworkPlayerModel = Resources.FindObjectsOfTypeAll<LocalNetworkPlayerModel>().FirstOrDefault();

            var platformUserModel = _localNetworkPlayerModel.GetField<IPlatformUserModel, LocalNetworkPlayerModel>("_platformUserModel");
            UserInfo = await platformUserModel.GetUserInfo();

            if (UserInfo == null)
            {
                Plugin.Log?.Error($"Failed to get local network player!");
                return;
            }

            Plugin.Log?.Info($"Got local network player (platform: {UserInfo.platform}, platformUserId: {UserInfo.platformUserId}, userName: {UserInfo.userName})");
        }
    }
}
