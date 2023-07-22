namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(PlayerVoteArea))]
internal static class PlayerVoteAreaPatch
{
    [HarmonyPatch(nameof(PlayerVoteArea.SetCosmetics)), HarmonyPostfix]
    internal static void SetCosmetics(PlayerVoteArea __instance, GameData.PlayerInfo playerInfo)
    {
        // MeetingHudPatch.UpdateNameplate(__instance, playerInfo.PlayerId);
    }
}