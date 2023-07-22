namespace AmongDogUs;

internal static class ModMapOptions
{
    internal static int MaxNumberOfMeetings = 10;
    internal static bool BlockSkippingInEmergencyMeetings = false;
    internal static bool NoVoteIsSelfVote = false;
    internal static bool HideOutOfSightNametags = false;
    internal static bool HidePlayerNames = false;
    internal static int RestrictDevices = 0;
    internal static float RestrictAdminTime = 600f;
    internal static float RestrictAdminTimeMax = 600f;
    internal static float RestrictCamerasTime = 600f;
    internal static float RestrictCamerasTimeMax = 600f;
    internal static float RestrictVitalsTime = 600f;
    internal static float RestrictVitalsTimeMax = 600f;

    internal static bool GhostsSeeRoles = true;
    internal static bool GhostsSeeTasks = true;
    internal static bool GhostsSeeVotes = true;
    internal static bool ShowRoleSummary = true;
    // internal static bool HideNameplates = false;
    internal static bool AllowParallelMedBayScans = false;
    internal static bool EnableCustomSounds = true;
    internal static bool ShowLighterDarker = false;
    // internal static bool EnableHorseMode = false;

    internal static int MeetingsCount = 0;
    internal static List<SurvCamera> CamerasToAdd = new();
    internal static List<Vent> VentsToSeal = new();
    internal static Dictionary<byte, PoolablePlayer> PlayerIcons = new();

    internal static void ClearAndReloadOptions()
    {
        MeetingsCount = 0;
        CamerasToAdd = new();
        VentsToSeal = new();
        PlayerIcons = new();

        MaxNumberOfMeetings = Mathf.RoundToInt(CustomOptionsHolder.MaxNumberOfMeetings.GetSelection());
        BlockSkippingInEmergencyMeetings = CustomOptionsHolder.BlockSkippingInEmergencyMeetings.GetBool();
        NoVoteIsSelfVote = CustomOptionsHolder.NoVoteIsSelfVote.GetBool();

        HideOutOfSightNametags = CustomOptionsHolder.HideOutOfSightNameTags.GetBool();
        HidePlayerNames = CustomOptionsHolder.HidePlayerNames.GetBool();

        RestrictDevices = CustomOptionsHolder.RestrictDevices.GetSelection();
        RestrictAdminTime = RestrictAdminTimeMax = CustomOptionsHolder.RestrictAdmin.GetFloat();
        RestrictCamerasTime = RestrictCamerasTimeMax = CustomOptionsHolder.RestrictCameras.GetFloat();
        RestrictVitalsTime = RestrictVitalsTimeMax = CustomOptionsHolder.RestrictVitals.GetFloat();
    }

    internal static void ReloadPluginOptions()
    {
        AllowParallelMedBayScans = CustomOptionsHolder.AllowParallelMedBayScans.GetBool();
        GhostsSeeRoles = Main.GhostsSeeRoles.Value;
        GhostsSeeTasks = Main.GhostsSeeTasks.Value;
        GhostsSeeVotes = Main.GhostsSeeVotes.Value;
        // HideNameplates = Main.HideNameplates.Value;
        ShowRoleSummary = Main.ShowRoleSummary.Value;
        EnableCustomSounds = Main.EnableCustomSounds.Value;
        ShowLighterDarker = Main.ShowLighterDarker.Value;
        // EnableHorseMode = Main.EnableHorseMode.Value;
        // HorseModePatch.ShouldAlwaysHorseAround.isHorseMode = UltimateModsPlugin.EnableHorseMode.Value;
    }

    internal static void ResetDeviceTimes()
    {
        RestrictAdminTime = RestrictAdminTimeMax;
        RestrictCamerasTime = RestrictCamerasTimeMax;
        RestrictVitalsTime = RestrictVitalsTimeMax;
    }

    internal static bool CanUseAdmin { get { return RestrictDevices == 0 || RestrictAdminTime > 0f; } }
    internal static bool CouldUseAdmin { get { return RestrictDevices == 0 || RestrictAdminTimeMax > 0f; } }
    internal static bool CanUseCameras { get { return RestrictDevices == 0 || RestrictCamerasTime > 0f; } }
    internal static bool CouldUseCameras { get { return RestrictDevices == 0 || RestrictCamerasTimeMax > 0f; } }
    internal static bool CanUseVitals { get { return RestrictDevices == 0 || RestrictVitalsTime > 0f; } }
    internal static bool CouldUseVitals { get { return RestrictDevices == 0 || RestrictVitalsTimeMax > 0f; } }
}