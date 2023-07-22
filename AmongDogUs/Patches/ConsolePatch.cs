namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(Console))]
internal static class ConsolePatch
{
    [HarmonyPatch(nameof(Console.CanUse)), HarmonyPrefix]
    internal static bool CanUse(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        canUse = couldUse = false;
        __result = float.MaxValue;

        //if (IsBlocked(__instance, pc.Object)) return false;
        if (__instance.AllowImpostor) return true;
        if (!pc.Object.HasFakeTasks()) return true;

        return false;
    }

    [HarmonyPatch(nameof(Console.Use)),HarmonyPrefix]
    internal static bool Use(Console __instance)
    {
        return !BlockButtonPatch.IsBlocked(__instance, PlayerControl.LocalPlayer);
    }
}