namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(HudManager))]
internal static class HudManagerPatch
{
    [HarmonyPatch(nameof(HudManager.Start)), HarmonyPostfix]
    internal static void SetupButton(HudManager __instance)
    {
        RoleData.MakeButtons(__instance);
        RoleData.SetCustomButtonCooldowns();
    }

    [HarmonyPatch(nameof(HudManager.OpenMeetingRoom)), HarmonyPrefix]
    internal static void OpenMeetingRoom()
    {
        AmongDogUs.OnMeetingStart();
    }

    [HarmonyPatch(nameof(HudManager.Update)), HarmonyPostfix]
    internal static void Update()
    {
        if (AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Started)
        {
            CustomButton.HudUpdate();

            if (AmongUsClient.Instance.AmHost && Input.GetKeyDown(KeyCode.F11))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UncheckedEndGame, SendOption.Reliable, -1);
                writer.Write((byte)CustomGameOverReason.ForceEnd);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.UncheckedEndGame((byte)CustomGameOverReason.ForceEnd);
            }
        }

        var FullScreen = GameObject.Find("FullScreen500(Clone)");
        if (FullScreen)
        {
            FullScreen.SetActive(false);
            Main.Logger.LogWarning("[WARNING] Crew Loading Animation was disabled with mod!");
        }
        var FullScreenC = GameObject.Find("FullScreen500(Clone)(Clone)");
        if (FullScreenC)
        {
            FullScreenC.SetActive(false);
            Main.Logger.LogWarning("[WARNING] Crew Loading Animation was disabled with mod!");
        }
    }

    // [HarmonyPatch(nameof(HudManager.SetHudActive)), HarmonyPostfix]
    // internal static void SetHudActive(HudManager __instance)
    // {
    //     FastDestroyableSingleton<HudManager>.Instance.transform.FindChild("TaskDisplay").FindChild("TaskPanel").gameObject.SetActive(true);
    // }
}
