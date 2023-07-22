namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(SabotageButton))]
internal static class SabotageButtonDoClickPatch
{
    [HarmonyPatch(nameof(SabotageButton.DoClick)), HarmonyPrefix]
    internal static bool DoClick(SabotageButton __instance)
    {
        if (!PlayerControl.LocalPlayer.IsNeutral()) return true;

        FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
        {
            Mode = MapOptions.Modes.Sabotage,
            AllowMovementWhileMapOpen = true
        });

        return false;
    }
}