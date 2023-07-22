namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(AmongUsClient))]
internal static class AmongUsClientOnPlayerJoinedPatch
{
    [HarmonyPatch(nameof(AmongUsClient.OnPlayerJoined)), HarmonyPostfix]
    internal static void OnPlayerJoined()
    {
        if (PlayerControl.LocalPlayer != null)
        {
            Helpers.ShareGameVersion();
        }
    }

    [HarmonyPatch(nameof(AmongUsClient.StartGame)),HarmonyPostfix]
    internal static void StartGame()
    {
        GameManager.Instance.LogicOptions.currentGameOptions.SetBool(BoolOptionNames.AnonymousVotes, true);
    }
}