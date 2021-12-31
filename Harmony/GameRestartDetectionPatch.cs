using System;
using HarmonyLib;
using ServerBrowser.Game;
using ServerBrowser.UI;
using Zenject;

namespace ServerBrowser.Harmony
{
    /// <summary>
    /// This patch lets us detect a game restart and work around some UI breakage.
    /// </summary>
    [HarmonyPatch(typeof(GameScenesManager), "ClearAndOpenScenes", MethodType.Normal)]
    public static class GameRestartDetectionPatch
    {
        private static GameScenesManager? _instance;
        
        public static void Prefix(GameScenesManager __instance, bool unloadAllScenes)
        {
            if (!unloadAllScenes)
                return;
            
            Plugin.Log.Debug("On game restart - pre - self disable");
            Plugin.Instance.OnDisable();

            _instance = __instance;
            __instance.transitionDidFinishEvent += HandleTransitionFinish;
        }

        private static void HandleTransitionFinish(ScenesTransitionSetupDataSO arg1, DiContainer arg2)
        {
            if (_instance is null)
                return;
            
            Plugin.Log.Debug("On game restart - post - self enable");
            Plugin.Instance.OnEnable();
            
            _instance.transitionDidFinishEvent -= HandleTransitionFinish;
        }
    }
}