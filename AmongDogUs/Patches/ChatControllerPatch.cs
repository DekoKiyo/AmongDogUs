namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(ChatController))]
internal static class ChatControllerPatch
{
    [HarmonyPatch(nameof(ChatController.Awake)), HarmonyPrefix]
    private static void FreeChat()
    {
        if (!EOSManager.Instance.isKWSMinor)
        {
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
            Main.Logger.Log(LogLevel.Info, "FreeChat is available now!");
        }
    }
}