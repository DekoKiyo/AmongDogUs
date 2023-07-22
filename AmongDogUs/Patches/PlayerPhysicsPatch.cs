namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(PlayerPhysics))]
internal static class PlayerPhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.Awake)), HarmonyPostfix]
    internal static void Postfix(PlayerPhysics __instance)
    {
        if (!__instance.body) return;
        __instance.body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
}