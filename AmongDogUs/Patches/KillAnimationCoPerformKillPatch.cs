namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
internal static class KillAnimationCoPerformKillPatch
{
    public static bool hideNextAnimation = false;

    public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source, [HarmonyArgument(1)] ref PlayerControl target)
    {
        if (hideNextAnimation) source = target;
        hideNextAnimation = false;
    }
}