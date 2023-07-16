namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(ChatController))]
public static class ChatControllerAwakePatch
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