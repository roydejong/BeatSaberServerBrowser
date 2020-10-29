using HarmonyLib;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyBrowserMod.Harmony
{

    [HarmonyPatch(typeof(MultiplayerModeSelectionViewController), "DidActivate", MethodType.Normal)]
    class MultiplayerModeSelectionPatch
    {
        static void Postfix(MultiplayerModeSelectionViewController __instance, bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            // Enable the "game browser" button (it was left in the game but unused currently)
            var btnGameBrowser = ReflectionUtil.GetField<Button, MultiplayerModeSelectionViewController>(__instance, "_gameBrowserButton");

            btnGameBrowser.enabled = true;
            btnGameBrowser.gameObject.SetActive(true);

            foreach (var comp in btnGameBrowser.GetComponents<Component>())
            {
                comp.gameObject.SetActive(true);
            }

            if (firstActivation)
            {
                var transform = btnGameBrowser.gameObject.transform;

                // Move the button up a bit
                transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + 0.25f,
                    transform.position.z
                );

                // Make it a little bit bigger
                transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

                // Change the label from "server browser" to "lobby browser" because consistency
                var label = btnGameBrowser.GetComponentInChildren<CurvedTextMeshPro>();

                if (label != null)
                {
                    label.SetText("Lobby Browser");
                }
            }
        }
    }
}
