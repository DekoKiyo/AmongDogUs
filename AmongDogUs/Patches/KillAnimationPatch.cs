namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(KillAnimation))]
internal static class KillAnimationPatch
{
    internal static bool hideNextAnimation = false;

    [HarmonyPatch(nameof(KillAnimation.CoPerformKill)), HarmonyPrefix]
    internal static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source, [HarmonyArgument(1)] ref PlayerControl target)
    {
        if (hideNextAnimation) source = target;
        hideNextAnimation = false;
    }
}