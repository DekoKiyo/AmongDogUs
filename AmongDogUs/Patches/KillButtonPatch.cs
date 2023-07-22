namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(KillButton))]
internal static class KillButtonPatch
{
    [HarmonyPatch(nameof(KillButton.DoClick)), HarmonyPrefix]
    internal static bool DoClick(KillButton __instance)
    {
        if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove)
        {
            bool showAnimation = true;

            // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
            MurderAttemptResult res = Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
            // Handle blank kill
            if (res == MurderAttemptResult.BlankKill)
            {
                PlayerControl.LocalPlayer.killTimer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            }

            __instance.SetTarget(null);
        }
        return false;
    }
}