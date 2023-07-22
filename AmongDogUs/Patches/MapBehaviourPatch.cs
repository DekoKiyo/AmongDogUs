namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(MapBehaviour))]
internal static class ShowImpostorMapPatch
{
    [HarmonyPatch(nameof(MapBehaviour.ShowSabotageMap)), HarmonyPrefix]
    internal static void Prefix(ref RoleTeamTypes __state)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.IsRole(RoleType.Jester) && CustomOptionsHolder.JesterCanSabotage.GetBool())
        {
            __state = player.Data.Role.TeamType;
            player.Data.Role.TeamType = RoleTeamTypes.Impostor;
        }
        if (player.IsRole(RoleType.CustomImpostor) && CustomOptionsHolder.CustomImpostorCanSabotage.GetBool())
        {
            __state = player.Data.Role.TeamType;
            player.Data.Role.TeamType = RoleTeamTypes.Impostor;
        }
    }

    [HarmonyPatch(nameof(MapBehaviour.ShowSabotageMap)), HarmonyPostfix]
    internal static void Postfix(ref RoleTeamTypes __state)
    {
        var player = PlayerControl.LocalPlayer;
        if (player.IsRole(RoleType.Jester) && CustomOptionsHolder.JesterCanSabotage.GetBool())
        {
            player.Data.Role.TeamType = __state;
        }
        if (player.IsRole(RoleType.CustomImpostor) && CustomOptionsHolder.CustomImpostorCanSabotage.GetBool())
        {
            player.Data.Role.TeamType = __state;
        }
    }
}