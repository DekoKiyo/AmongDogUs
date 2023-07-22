namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(ExileController))]
internal static class ExileControllerPatch
{
    internal static GameData.PlayerInfo lastExiled;
    [HarmonyPatch(nameof(ExileController.Begin)), HarmonyPrefix, HarmonyPriority(Priority.First)]
    internal static void Begin(ExileController __instance, [HarmonyArgument(0)] ref GameData.PlayerInfo exiled, [HarmonyArgument(1)] bool tie)
    {
        lastExiled = exiled;

        // 1 = Reset per turn
        if (ModMapOptions.RestrictDevices is 1) ModMapOptions.ResetDeviceTimes();
    }

    internal static bool ShuffleTimingIsMeetingEnd = false;
    [HarmonyPatch(nameof(ExileController.WrapUp)), HarmonyPostfix]
    internal static void WrapUp(ExileController __instance)
    {
        WrapUpPostfix(__instance.exiled);
    }

    internal static void WrapUpPostfix(GameData.PlayerInfo exiled)
    {
        if (exiled != null)
        {
            var p = exiled.Object;
            // Jester win condition
            if (p.IsRole(RoleType.Jester))
            {
                if ((Jester.HasTasks && Jester.TasksComplete(p)) || !Jester.HasTasks)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UncheckedEndGame, SendOption.Reliable, -1);
                    writer.Write((byte)CustomGameOverReason.JesterExiled);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.UncheckedEndGame((byte)CustomGameOverReason.JesterExiled);
                }
            }
        }

        // Reset custom button timers where necessary
        CustomButton.MeetingEndedUpdate();

        // Custom role post-meeting functions
        AmongDogUs.OnMeetingEnd();

        if (AirShipPatch.IsShuffleElecDoors && ShuffleTimingIsMeetingEnd) AirShipPatch.InitializeElecDoor();

        // Remove all DeadBodies
        DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < array.Length; i++)
        {
            Object.Destroy(array[i].gameObject);
        }
    }
}

[HarmonyPatch(typeof(AirshipExileController))]
internal static class AirshipExileControllerPatch
{
    [HarmonyPatch(nameof(AirshipExileController.WrapUpAndSpawn)), HarmonyPostfix]
    internal static void WrapUpAndSpawn(AirshipExileController __instance)
    {
        ExileControllerPatch.WrapUpPostfix(__instance.exiled);
    }
}