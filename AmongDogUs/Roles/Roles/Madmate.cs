namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Madmate : RoleBase<Madmate>
{
    public Madmate()
    {
        RoleType = roleType = RoleType.Madmate;
    }

    internal static bool CanDieToSheriffOrYakuza { get { return CustomOptionsHolder.MadmateCanDieToSheriffOrYakuza.GetBool(); } }
    internal static bool CanUseVents { get { return CustomOptionsHolder.MadmateCanEnterVents.GetBool(); } }
    internal static bool CanMoveInVents { get { return CustomOptionsHolder.MadmateCanMoveInVents.GetBool(); } }
    internal static bool CanSabotage { get { return CustomOptionsHolder.MadmateCanSabotage.GetBool(); } }
    internal static bool HasImpostorVision { get { return CustomOptionsHolder.MadmateHasImpostorVision.GetBool(); } }
    internal static bool CanFixO2 { get { return CustomOptionsHolder.MadmateCanFixO2.GetBool(); } }
    internal static bool CanFixComms { get { return CustomOptionsHolder.MadmateCanFixComms.GetBool(); } }
    internal static bool CanFixReactor { get { return CustomOptionsHolder.MadmateCanFixReactor.GetBool(); } }
    internal static bool CanFixBlackout { get { return CustomOptionsHolder.MadmateCanFixBlackout.GetBool(); } }
    internal static bool HasTasks { get { return CustomOptionsHolder.MadmateHasTasks.GetBool(); } }
    internal static int CommonTasksCount { get { return CustomOptionsHolder.MadmateTasksCount.CommonTasks; } }
    internal static int ShortTasksCount { get { return CustomOptionsHolder.MadmateTasksCount.ShortTasks; } }
    internal static int LongTasksCount { get { return CustomOptionsHolder.MadmateTasksCount.LongTasks; } }
    internal static bool CanKnowImpostorsTaskEnd { get { return CustomOptionsHolder.MadmateCanKnowImpostorWhenTasksEnded.GetBool(); } }
    internal static bool CanWinTaskEnd { get { return CustomOptionsHolder.MadmateCanWinWhenTaskEnded.GetBool(); } }


    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    internal class BeginCrewmatePatch
    {
        internal static void Postfix(ShipStatus __instance)
        {
            if (HasTasks && PlayerControl.LocalPlayer.IsRole(RoleType.Madmate))
            {
                Local.AssignTasks();
            }
        }
    }

    internal void AssignTasks()
    {
        player.GenerateAndAssignTasks(CommonTasksCount, ShortTasksCount, LongTasksCount);
    }

    internal static bool KnowsImpostors(PlayerControl player)
    {
        return CanKnowImpostorsTaskEnd && TasksComplete(player);
    }

    internal static bool TasksComplete(PlayerControl player)
    {
        if (!HasTasks) return false;

        int counter = 0;
        int totalTasks = CommonTasksCount + LongTasksCount + ShortTasksCount;
        if (totalTasks == 0) return true;
        foreach (var task in player.Data.Tasks)
        {
            if (task.Complete)
            {
                counter++;
            }
        }
        return counter >= totalTasks;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm) { }
    internal override void SetButtonCooldowns() { }

    internal override void Clear()
    {
        players = new List<Madmate>();
    }
}