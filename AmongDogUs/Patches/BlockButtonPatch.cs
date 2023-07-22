namespace AmongDogUs.Patches;

[HarmonyPatch]
internal static class BlockButtonPatch
{
    internal static bool IsBlocked(PlayerTask task, PlayerControl pc)
    {
        if (task == null || pc == null || pc != PlayerControl.LocalPlayer) return false;

        bool isLights = task.TaskType == TaskTypes.FixLights;
        bool isComms = task.TaskType == TaskTypes.FixComms;
        bool isReactor = task.TaskType == TaskTypes.StopCharles || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.ResetReactor;
        bool isO2 = task.TaskType == TaskTypes.RestoreOxy;

        if (pc.IsRole(RoleType.Madmate) && isLights && !Madmate.CanFixBlackout || (isReactor && !Madmate.CanFixReactor) || (isO2 && !Madmate.CanFixO2) || (isComms && !Madmate.CanFixComms))
        {
            return true;
        }

        return false;
    }

    internal static bool IsBlocked(Console console, PlayerControl pc)
    {
        if (console == null || pc == null || pc != PlayerControl.LocalPlayer)
        {
            return false;
        }

        PlayerTask task = console.FindTask(pc);
        return IsBlocked(task, pc);
    }

    internal static bool IsBlocked(SystemConsole console, PlayerControl pc)
    {
        if (console == null || pc == null || pc != PlayerControl.LocalPlayer)
        {
            return false;
        }

        string name = console.name;
        bool isSecurity = name == "task_cams" || name == "Surv_Panel" || name == "SurvLogConsole" || name == "SurvConsole";
        bool isVitals = name == "panel_vitals";

        if ((isSecurity && !ModMapOptions.CanUseCameras) || (isVitals && !ModMapOptions.CanUseVitals)) return true;
        return false;
    }

    internal static bool IsBlocked(IUsable target, PlayerControl pc)
    {
        if (target == null) return false;

        Console targetConsole = target.TryCast<Console>();
        SystemConsole targetSysConsole = target.TryCast<SystemConsole>();
        MapConsole targetMapConsole = target.TryCast<MapConsole>();
        if ((targetConsole != null && IsBlocked(targetConsole, pc)) ||
            (targetSysConsole != null && IsBlocked(targetSysConsole, pc)) ||
            (targetMapConsole != null && !ModMapOptions.CanUseAdmin))
        {
            return true;
        }
        return false;
    }
}