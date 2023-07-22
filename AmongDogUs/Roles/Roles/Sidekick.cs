namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Sidekick : RoleBase<Sidekick>
{
    internal static CustomButton SidekickKillButton;
    public Sidekick()
    {
        RoleType = roleType = RoleType.Sidekick;
    }

    internal static PlayerControl CurrentTarget;

    internal static float Cooldown { get { return CustomOptionsHolder.JackalKillCooldown.GetFloat(); } }
    internal static bool CanUseVents { get { return CustomOptionsHolder.SidekickCanUseVents.GetBool(); } }
    internal static bool CanKill { get { return CustomOptionsHolder.SidekickCanKill.GetBool(); } }
    internal static bool PromotesToJackal { get { return CustomOptionsHolder.SidekickPromotesToJackal.GetBool(); } }
    internal static bool HasImpostorVision { get { return CustomOptionsHolder.JackalAndSidekickHaveImpostorVision.GetBool(); } }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.Sidekick))
        {
            var BlockTarget = new List<PlayerControl>();
            if (Jackal.players != null) foreach (var jackal in Jackal.AllPlayers) BlockTarget.Add(jackal);

            CurrentTarget = PlayerControlPatch.SetTarget(unTargetablePlayers: BlockTarget);
            if (CanKill) PlayerControlPatch.SetPlayerOutline(CurrentTarget, ImpostorRed);
        }
    }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        SidekickKillButton = new(
            () =>
            {
                if (Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, CurrentTarget) == MurderAttemptResult.SuppressKill) return;

                SidekickKillButton.Timer = SidekickKillButton.MaxTimer;
                CurrentTarget = null;
            },
            () =>
            {
                return CanKill && PlayerControl.LocalPlayer.IsRole(RoleType.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { SidekickKillButton.Timer = SidekickKillButton.MaxTimer; },
            hm.KillButton.graphic.sprite,
            ButtonPositions.RightTop,
            hm,
            hm.KillButton,
            KeyCode.Q,
            false
        );
    }
    internal override void SetButtonCooldowns()
    {
        SidekickKillButton.MaxTimer = Cooldown;
    }

    internal override void Clear()
    {
        players = new List<Sidekick>();

        CurrentTarget = null;
    }
}