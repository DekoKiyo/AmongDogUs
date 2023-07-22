namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(GameData))]
internal static class HandleDisconnectPatch
{
    [HarmonyPatch(nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) }), HarmonyPostfix]
    internal static void HandleDisconnect(GameData __instance, PlayerControl player, DisconnectReasons reason)
    {
        if (AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Started)
        {
            Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
            Modifier.allModifiers.Do(x => x.HandleDisconnect(player, reason));
            finalStatuses[player.PlayerId] = EFinalStatus.Disconnected;
        }
    }
}