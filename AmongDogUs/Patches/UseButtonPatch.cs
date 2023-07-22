namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(UseButton))]
internal static class UseButtonPatch
{
    [HarmonyPatch(nameof(UseButton.SetTarget)), HarmonyPrefix]
    internal static bool SetTarget(UseButton __instance, [HarmonyArgument(0)] IUsable target)
    {
        PlayerControl pc = PlayerControl.LocalPlayer;
        __instance.enabled = true;

        if (BlockButtonPatch.IsBlocked(target, pc))
        {
            __instance.currentTarget = null;
            __instance.buttonLabelText.text = ModResources.ButtonBlocked;
            __instance.enabled = false;
            __instance.graphic.color = Palette.DisabledClear;
            __instance.graphic.material.SetFloat("_Desat", 0f);
            return false;
        }

        __instance.currentTarget = target;
        return true;
    }
}