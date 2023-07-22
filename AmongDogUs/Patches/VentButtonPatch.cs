namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(VentButton))]
internal static class VentButtonPatch
{
    [HarmonyPatch(nameof(VentButton.DoClick)), HarmonyPrefix]
    internal static bool DoClick(VentButton __instance)
    {
        // Manually modifying the VentButton to use Vent.Use again in order to trigger the Vent.Use prefix patch
        __instance.currentTarget?.Use();
        return false;
    }
}