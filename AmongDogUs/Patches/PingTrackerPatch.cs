namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(PingTracker))]
internal static class PingTrackerPatch
{
    [HarmonyPatch(nameof(PingTracker.Update)), HarmonyPostfix]
    internal static void Update(PingTracker __instance)
    {
        __instance.text.text = $"<size=150%><color=#59a2f3>AmongDogUs</color></size>\nVer.{Main.PLUGIN_VERSION}\n<size=75%>Developer: DekoKiyo</size>\n{__instance.text.text}";
        __instance.text.alignment = TextAlignmentOptions.TopRight;
    }
}