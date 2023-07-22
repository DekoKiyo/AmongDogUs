namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(TranslationController))]
internal static class ExileControllerMessagePatch
{
    [HarmonyPatch(nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    internal static void GetString(ref string __result, [HarmonyArgument(0)] StringNames id)
    {
        try
        {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null)
            {
                PlayerControl player = Helpers.PlayerById(ExileController.Instance.exiled.Object.PlayerId);
                if (player == null) return;
                List<RoleInfo> infos = RoleInfoList.GetRoleInfoForPlayer(player);
                RoleInfo roleInfo = infos.Where(info => info.RoleId != RoleType.NoRole).FirstOrDefault();
                // Exile role text
                if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP)
                {
                    __result = string.Format(ModResources.ExilePlayer, player.Data.PlayerName, roleInfo.Name);
                }
                // Hide Number of remaining impostors on Jester win
                if (id is StringNames.ImpostorsRemainP or StringNames.ImpostorsRemainS)
                {
                    if (player.IsRole(RoleType.Jester)) __result = "";
                }
            }
        }
        catch
        {
            // pass - Hopefully prevent leaving while exiling to soft lock game
        }
    }
}