using IPA.Utilities;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ServerBrowser.Game
{
    public static class MpLocalPlayer
    {
        public static LocalNetworkPlayerModel LocalNetworkPlayerModel
        {
            get;
            private set;
        }

        public static UserInfo UserInfo
        {
            get;
            private set;
        }

        public static UserInfo.Platform? Platform => UserInfo?.platform;
        public static string PlatformUserId => UserInfo?.platformUserId;
        public static string UserName => UserInfo?.userName;

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
            /** 
             * Note: The game creates one local player in MainSystemInit.InstallBindings(), so
             *   we have only one instance that we can use continuously, it won't change.
             */
            LocalNetworkPlayerModel = Resources.FindObjectsOfTypeAll<LocalNetworkPlayerModel>().FirstOrDefault();

            var platformUserModel = LocalNetworkPlayerModel.GetField<IPlatformUserModel, LocalNetworkPlayerModel>("_platformUserModel");
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
