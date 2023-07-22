namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Jackal : RoleBase<Jackal>
{
    internal static CustomButton JackalKillButton;
    internal static CustomButton JackalMakeSidekickButton;
    public Jackal()
    {
        RoleType = roleType = RoleType.Jackal;
        CanSidekick = CanCreateSidekick;
    }

    internal static PlayerControl CurrentTarget;
    internal static Sprite JackalSidekickButtonSprite;
    internal static List<PlayerControl> BlockTarget = new();

    internal static float Cooldown { get { return CustomOptionsHolder.JackalKillCooldown.GetFloat(); } }
    internal static float CreateSideKickCooldown { get { return CustomOptionsHolder.JackalCreateSidekickCooldown.GetFloat(); } }
    internal static bool CanUseVents { get { return CustomOptionsHolder.JackalCanUseVents.GetBool(); } }
    internal static bool CanCreateSidekick { get { return CustomOptionsHolder.JackalCanCreateSidekick.GetBool(); } }
    internal static bool JackalPromotedFromSidekickCanCreateSidekick { get { return CustomOptionsHolder.JackalPromotedFromSidekickCanCreateSidekick.GetBool(); } }
    internal static bool HasImpostorVision { get { return CustomOptionsHolder.JackalAndSidekickHaveImpostorVision.GetBool(); } }
    internal static bool CanSidekick = true;

    internal static Sprite GetButtonSprite()
    {
        if (JackalSidekickButtonSprite) return JackalSidekickButtonSprite;
        JackalSidekickButtonSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.JackalSidekickButton, 115f);
        return JackalSidekickButtonSprite;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.Jackal))
        {
            foreach (var pc in PlayerControl.AllPlayerControls) if (pc.IsTeamJackal()) BlockTarget.Add(pc);

            CurrentTarget = PlayerControlPatch.SetTarget(unTargetablePlayers: BlockTarget);
            PlayerControlPatch.SetPlayerOutline(CurrentTarget, JackalBlue);
        }
    }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null)
    {
        if (Sidekick.PromotesToJackal &&
                PlayerControl.LocalPlayer.IsRole(RoleType.Sidekick) &&
                PlayerControl.LocalPlayer.IsAlive())
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.SidekickPromotes, SendOption.Reliable, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SidekickPromotes(PlayerControl.LocalPlayer.PlayerId);
        }
    }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        JackalKillButton = new(
            () =>
            {
                if (Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, CurrentTarget) == MurderAttemptResult.SuppressKill) return;

                JackalKillButton.Timer = JackalKillButton.MaxTimer;
                CurrentTarget = null;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { JackalKillButton.Timer = JackalKillButton.MaxTimer; },
            hm.KillButton.graphic.sprite,
            ButtonPositions.RightTop,
            hm,
            hm.KillButton,
            KeyCode.Q,
            false
        );

        JackalMakeSidekickButton = new(
            () =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.JackalCreatesSidekick, SendOption.Reliable, -1);
                writer.Write(CurrentTarget.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.JackalCreatesSidekick(CurrentTarget.PlayerId);
            },
            () => { return CanSidekick && PlayerControl.LocalPlayer.IsRole(RoleType.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return CanSidekick && CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { JackalMakeSidekickButton.Timer = JackalMakeSidekickButton.MaxTimer; },
            GetButtonSprite(),
            ButtonPositions.CenterTop,
            hm,
            hm.KillButton,
            KeyCode.F,
            false
        )
        {
            ButtonText = ModResources.JackalSidekickText
        };
    }
    internal override void SetButtonCooldowns()
    {
        JackalKillButton.MaxTimer = Cooldown;
        JackalMakeSidekickButton.MaxTimer = CreateSideKickCooldown;
    }

    internal override void Clear()
    {
        players = new List<Jackal>();

        CurrentTarget = null;
    }
}