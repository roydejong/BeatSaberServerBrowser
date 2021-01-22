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

            switch (UserInfo.platform)
            {
                case UserInfo.Platform.Oculus:
                    Plugin.PlatformId = Plugin.PLATFORM_OCULUS;
                    break;
                case UserInfo.Platform.Steam:
                    Plugin.PlatformId = Plugin.PLATFORM_STEAM;
                    break;
            }
        }
    }
}
