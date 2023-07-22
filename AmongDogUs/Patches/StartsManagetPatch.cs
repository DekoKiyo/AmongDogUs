namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(StatsManager))]
internal static class StatsManagerPatch
{
    [HarmonyPatch(nameof(StatsManager.AmBanned), MethodType.Getter), HarmonyPostfix]
    internal static void SwitchBANFalse(out bool __result)
    {
        __result = false;
        Main.Logger.Log(LogLevel.Info, "Switched BAN status to false.");
    }
}