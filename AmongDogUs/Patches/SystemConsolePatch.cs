namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(SystemConsole))]
internal static class SystemConsolePatch
{
    [HarmonyPatch(nameof(SystemConsole.CanUse)), HarmonyPrefix]
    internal static bool CanUse(ref float __result, SystemConsole __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;
        __result = float.MaxValue;
        if (BlockButtonPatch.IsBlocked(__instance, pc.Object)) return false;

        return true;
    }

    [HarmonyPatch(nameof(SystemConsole.Use)), HarmonyPrefix]
    internal static bool Use(SystemConsole __instance)
    {
        return !BlockButtonPatch.IsBlocked(__instance, PlayerControl.LocalPlayer);
    }
}