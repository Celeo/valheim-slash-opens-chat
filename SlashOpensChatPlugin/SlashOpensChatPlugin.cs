using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace SlashOpensChat
{
    [BepInPlugin("com.celeo.valheim.slashopenschat", "Slash Opens Chat", "1.0.0.0")]
    public class SlashOpensChatPlugin : BaseUnityPlugin
    {
        private static Harmony harmonyInstance;
        private static bool chatJustOpened = false;

        void Awake()
        {
            Debug.Log("SlashOpensChat plugin initialized");
            harmonyInstance = Harmony.CreateAndPatchAll(
                    Assembly.GetExecutingAssembly(),
                    "com.celeo.valheim.slashopenschat"
            );
        }

        void OnDestroy()
        {
            harmonyInstance.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Player), "Update")]
        public static class PlayerPatch
        {
            private static bool ChatAlreadyOpen()
            {
                return Console.IsVisible()
                    || TextInput.IsVisible()
                    || Minimap.InTextInput()
                    || Menu.IsVisible()
                    || Chat.instance.m_input.gameObject.activeSelf;
            }

            private static void Postfix(Player __instance)
            {
                if (Player.m_localPlayer != __instance)
                {
                    return;
                }
                if (Input.GetKeyDown(KeyCode.Slash) && !ChatAlreadyOpen())
                {
                    // Basically do what Chat::Update() does when the enter key is pressed
                    Chat.instance.GetType().GetField("m_hideTimer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Chat.instance, 0f);
                    Chat.instance.m_chatWindow.gameObject.SetActive(value: true);
                    Chat.instance.m_input.gameObject.SetActive(value: true);
                    Chat.instance.m_input.ActivateInputField();

                    // Put the slash into the input
                    Chat.instance.m_input.text = "/";

                    // Set a flag so the chat text position can be updated
                    chatJustOpened = true;
                }
            }
        }

        [HarmonyPatch(typeof(Chat), "LateUpdate")]
        public static class ChatPatch
        {
            private static void Postfix(Chat __instance)
            {
                if (chatJustOpened)
                {
                    Chat.instance.m_input.MoveTextEnd(true);
                    chatJustOpened = false;
                }
            }
        }
    }
}