namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(ShipStatus))]
internal static class AirShipPatch
{
    internal static bool TopLeftVert = false;
    internal static bool TopLeftHort = false;
    internal static bool BottomHort = false;
    internal static bool TopCenterHort = false;
    internal static bool LeftVert = false;
    internal static bool RightVert = false;
    internal static bool TopRightVert = false;
    internal static bool TopRightHort = false;
    internal static bool BottomRightVert = false;
    internal static bool BottomRightHort = false;
    internal static bool LeftDoorTop = false;
    internal static bool LeftDoorBottom = false;
    internal static bool IsShuffleElecDoors = false;

    [HarmonyPatch(nameof(ShipStatus.Start)), HarmonyPostfix]
    internal static void StartElecDoorSystem(ShipStatus __instance)
    {
        if (Helpers.GetMapId() is not 4) return;

        Main.Logger.LogInfo("エレキドアシステム開始");

        ExileControllerPatch.ShuffleTimingIsMeetingEnd = false;
        PlayerControlPatch.ShuffleTimingIsKilled = false;
        VentPatch.ShuffleTimingIsEnterVents = false;

        switch (AirShipOption.AirShipElecDoorOptData)
        {
            case 0:
                IsShuffleElecDoors = false;
                break;
            case 1:
                IsShuffleElecDoors = false;
                GetStaticDoor("TopLeftVert").SetOpen(true);
                GetStaticDoor("TopLeftHort").SetOpen(true);
                GetStaticDoor("BottomHort").SetOpen(true);
                GetStaticDoor("TopCenterHort").SetOpen(true);
                GetStaticDoor("LeftVert").SetOpen(true);
                GetStaticDoor("RightVert").SetOpen(true);
                GetStaticDoor("TopRightVert").SetOpen(true);
                GetStaticDoor("TopRightHort").SetOpen(true);
                GetStaticDoor("BottomRightHort").SetOpen(true);
                GetStaticDoor("BottomRightVert").SetOpen(true);
                GetStaticDoor("LeftDoorTop").SetOpen(true);
                GetStaticDoor("LeftDoorBottom").SetOpen(true);
                break;
            case 2:
                IsShuffleElecDoors = true;
                switch (AirShipOption.AirShipElecDoorTiming)
                {
                    case 0: // On Meeting End
                        ExileControllerPatch.ShuffleTimingIsMeetingEnd = true;
                        break;
                    case 1: // Someone killed
                        PlayerControlPatch.ShuffleTimingIsKilled = true;
                        break;
                    case 2: // Someone Enter Vents
                        VentPatch.ShuffleTimingIsEnterVents = true;
                        break;
                }
                break;
            case 3:
                IsShuffleElecDoors = false;
                GetStaticDoor("TopLeftVert").SetOpen(TopLeftVert);
                GetStaticDoor("TopLeftHort").SetOpen(TopLeftHort);
                GetStaticDoor("BottomHort").SetOpen(BottomHort);
                GetStaticDoor("TopCenterHort").SetOpen(TopCenterHort);
                GetStaticDoor("LeftVert").SetOpen(LeftVert);
                GetStaticDoor("RightVert").SetOpen(RightVert);
                GetStaticDoor("TopRightVert").SetOpen(TopRightVert);
                GetStaticDoor("TopRightHort").SetOpen(TopRightHort);
                GetStaticDoor("BottomRightHort").SetOpen(BottomRightHort);
                GetStaticDoor("BottomRightVert").SetOpen(BottomRightVert);
                GetStaticDoor("LeftDoorTop").SetOpen(LeftDoorTop);
                GetStaticDoor("LeftDoorBottom").SetOpen(LeftDoorBottom);

                if (PlayerControl.LocalPlayer.IsCrew()) PlayerControl.LocalPlayer.ClearFixBreakerTask();
                break;
        }
    }

    internal static StaticDoor GetStaticDoor(string name)
    {
        foreach (var doors in Object.FindObjectsOfType(Il2CppType.Of<StaticDoor>()))
        {
            if (doors.name != name) continue;

            return doors.CastFast<StaticDoor>();
        }
        return null;
    }

    internal static void InitializeElecDoor()
    {
        ShipStatus.Instance.Systems[SystemTypes.Decontamination].CastFast<ElectricalDoors>().Initialize();
    }

    internal static void ClearFixBreakerTask(this PlayerControl player)
    {
        if (player == null) return;
        foreach (var task in player.myTasks)
        {
            if (task.name == FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ResetBreakers))
            {
                PlayerTask playerTask = task;
                playerTask.OnRemove();
                Object.Destroy(playerTask.gameObject);
            }
        }
    }
}

// TopLeftVert
// TopLeftHort
// BottomHort
// TopCenterHort
// LeftVert
// RightVert
// TopRightVert
// TopRightHort
// BottomRightVert
// BottomRightHort
// LeftDoorTop
// LeftDoorBottom