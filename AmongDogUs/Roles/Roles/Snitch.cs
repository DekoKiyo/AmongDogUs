namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Snitch : RoleBase<Snitch>
{
    public Snitch()
    {
        RoleType = roleType = RoleType.Snitch;
    }

    internal static List<CustomArrow> LocalArrows = new();
    internal static int TaskCountForReveal { get { return Mathf.RoundToInt(CustomOptionsHolder.SnitchLeftTasksForReveal.GetFloat()); } }
    internal static bool IncludeTeamJackal { get { return CustomOptionsHolder.SnitchIncludeTeamJackal.GetBool(); } }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate()
    {
        if (LocalArrows == null) return;

        foreach (CustomArrow arrow in LocalArrows) arrow.arrow.SetActive(false);

        foreach (var snitch in AllPlayers)
        {
            var (PlayerCompleted, PlayerTotal) = TasksHandler.TaskInfo(snitch.Data);
            int NumberOfTasks = PlayerTotal - PlayerCompleted;

            if (NumberOfTasks <= TaskCountForReveal)
            {
                if (PlayerControl.LocalPlayer.Data.Role.IsImpostor || (IncludeTeamJackal && (PlayerControl.LocalPlayer.IsRole(RoleType.Jackal) || PlayerControl.LocalPlayer.IsRole(RoleType.Sidekick))))
                {
                    if (LocalArrows.Count == 0) LocalArrows.Add(new(SnitchGreen));
                    if (LocalArrows.Count != 0 && LocalArrows[0] != null)
                    {
                        LocalArrows[0].arrow.SetActive(true);
                        LocalArrows[0].image.color = SnitchGreen;
                        LocalArrows[0].Update(snitch.transform.position);
                    }
                }
                else if (PlayerControl.LocalPlayer.IsRole(RoleType.Snitch))
                {
                    int arrowIndex = 0;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    {
                        bool ArrowForImpostor = p.Data.Role.IsImpostor;
                        bool ArrowForTeamJackal = IncludeTeamJackal && (p.IsRole(RoleType.Jackal) || p.IsRole(RoleType.Sidekick));

                        // Update the arrows' color every time bc things go weird when you add a sidekick or someone dies
                        Color c = ImpostorRed;
                        if (ArrowForTeamJackal) c = JackalBlue;

                        if (!p.Data.IsDead && (ArrowForImpostor || ArrowForTeamJackal))
                        {
                            if (arrowIndex >= LocalArrows.Count)
                            {
                                LocalArrows.Add(new(c));
                            }
                            if (arrowIndex < LocalArrows.Count && LocalArrows[arrowIndex] != null)
                            {
                                LocalArrows[arrowIndex].image.color = c;
                                LocalArrows[arrowIndex].arrow.SetActive(true);
                                LocalArrows[arrowIndex].Update(p.transform.position, c);
                            }
                            arrowIndex++;
                        }
                    }
                }
            }
        }
    }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm) { }
    internal override void SetButtonCooldowns() { }

    internal override void Clear()
    {
        players = new List<Snitch>();

        if (LocalArrows != null) foreach (CustomArrow Arrow in LocalArrows)
                if (Arrow?.arrow != null) Object.Destroy(Arrow.arrow);
    }
}