namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Jester : RoleBase<Jester>
{
    public Jester()
    {
        RoleType = roleType = RoleType.Jester;
    }

    internal static bool CanCallEmergency { get { return CustomOptionsHolder.JesterCanEmergencyMeeting.GetBool(); } }
    internal static bool CanUseVents { get { return CustomOptionsHolder.JesterCanUseVents.GetBool(); } }
    internal static bool CanMoveInVents { get { return CustomOptionsHolder.JesterCanMoveInVents.GetBool(); } }
    internal static bool CanSabotage { get { return CustomOptionsHolder.JesterCanSabotage.GetBool(); } }
    internal static bool HasImpostorVision { get { return CustomOptionsHolder.JesterHasImpostorVision.GetBool(); } }
    internal static bool HasTasks { get { return CustomOptionsHolder.JesterMustFinishTasks.GetBool(); } }
    internal static int NumCommonTasks { get { return CustomOptionsHolder.JesterTasks.CommonTasks; } }
    internal static int NumLongTasks { get { return CustomOptionsHolder.JesterTasks.LongTasks; } }
    internal static int NumShortTasks { get { return CustomOptionsHolder.JesterTasks.ShortTasks; } }

    internal void AssignTasks()
    {
        player.GenerateAndAssignTasks(NumCommonTasks, NumShortTasks, NumLongTasks);
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        internal static void Postfix(ShipStatus __instance)
        {
            if (HasTasks)
            {
                Local.AssignTasks();
            }
        }
    }

    internal static bool TasksComplete(PlayerControl p)
    {
        int FinishedTasks = 0;
        int TasksCount = NumCommonTasks + NumLongTasks + NumShortTasks;
        if (TasksCount == 0) return true;
        foreach (var task in p.Data.Tasks)
        {
            if (task.Complete)
            {
                FinishedTasks++;
            }
        }
        return FinishedTasks >= TasksCount;
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
        players = new List<Jester>();
    }
}