namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(StatsManager))]
public static class AmBannedPatch
{
    [HarmonyPatch(nameof(StatsManager.AmBanned), MethodType.Getter), HarmonyPostfix]
    public static void SwitchBANFalse(out bool __result)
    {
        __result = false;
        Main.Logger.Log(LogLevel.Info, "Switched BAN status to false.");
    }
}