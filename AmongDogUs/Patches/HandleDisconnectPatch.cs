namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
internal static class HandleDisconnectPatch
{
    public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
    {
        if (AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Started)
        {
            Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
            Modifier.allModifiers.Do(x => x.HandleDisconnect(player, reason));
            finalStatuses[player.PlayerId] = EFinalStatus.Disconnected;
        }
    }
}