using HarmonyLib;
using HMUI;
using IPA.Utilities;
using LobbyBrowserMod.Core;
using System.CodeDom;
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
                // Move the button up a bit
                var transform = btnGameBrowser.gameObject.transform;
                transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + 0.25f,
                    transform.position.z
                );
                transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                // Change the label from "server browser" to "lobby browser"
                var label = btnGameBrowser.GetComponentInChildren<CurvedTextMeshPro>();

                if (label != null)
                {
                    label.SetText("Lobby Browser");
                    label.colorGradient = new TMPro.VertexGradient(Color.red, Color.blue, Color.red, Color.blue);
                }
            }
        }
    }
}
