namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(PingTracker))]
internal static class PingTrackerPatch
{
    [HarmonyPatch(nameof(PingTracker.Update)), HarmonyPostfix]
    internal static void Update(PingTracker __instance)
    {
        __instance.text.text = $"<size=150%><color=#59a2f3>AmongDogUs</color></size>\nVer.{Main.PLUGIN_VERSION}\n<size=75%>Developer: DekoKiyo</size>\n{__instance.text.text}";
        __instance.text.alignment = TextAlignmentOptions.TopRight;

        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
        {
            if (PlayerControl.LocalPlayer.Data.IsDead) __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.0f, 0.1f, 0.5f);
            else __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(1.2f, 0.05f, 0.5f);
        }
        else __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.8f, 0.05f, 0.5f);
    }
}