namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(MedScanMinigame))]
internal static class MedScanMinigamePatch
{
    [HarmonyPatch(nameof(MedScanMinigame.FixedUpdate)), HarmonyPrefix]
    internal static void FixedUpdate(MedScanMinigame __instance)
    {
        if (ModMapOptions.AllowParallelMedBayScans)
        {
            __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
            __instance.medscan.UsersList.Clear();
        }
    }
}