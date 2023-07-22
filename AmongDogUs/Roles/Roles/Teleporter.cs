namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Teleporter : RoleBase<Teleporter>
{
    internal static CustomButton TeleportButton;
    public Teleporter()
    {
        RoleType = roleType = RoleType.Teleporter;
    }

    internal enum TeleportTarget
    {
        AliveAllPlayer = 0,
        Crewmate = 1,
    }

    internal static Sprite TeleportButtonSprite;
    internal static float Cooldown { get { return CustomOptionsHolder.TeleporterButtonCooldown.GetFloat(); } }
    internal static TeleportTarget TeleportTo { get { return (TeleportTarget)CustomOptionsHolder.TeleporterTeleportTo.GetSelection(); } }

    internal static Sprite GetButtonSprite()
    {
        if (TeleportButtonSprite) return TeleportButtonSprite;
        TeleportButtonSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.TeleporterTeleportButton, 115f);
        return TeleportButtonSprite;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        TeleportButton = new(
            () =>
            {
                List<PlayerControl> Target = new();
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    switch (TeleportTo)
                    {
                        case TeleportTarget.AliveAllPlayer:
                            if (pc.IsAlive() && pc.CanMove)
                            {
                                Target.Add(pc);
                            }
                            break;
                        case TeleportTarget.Crewmate:
                            if (pc.IsAlive() && pc.CanMove && pc.IsCrew())
                            {
                                Target.Add(pc);
                            }
                            break;
                    }
                }
                var player = Helpers.GetRandom(Target);
                MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.TeleporterTeleport, SendOption.Reliable, -1);
                Writer.Write(player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(Writer);
                RPCProcedure.TeleporterTeleport(player.PlayerId);
                TeleportButton.Timer = Cooldown;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.Teleporter) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () => { TeleportButton.Timer = TeleportButton.MaxTimer = Cooldown; },
            GetButtonSprite(),
            ButtonPositions.LeftTop,
            hm,
            hm.KillButton,
            KeyCode.F,
            false
        )
        {
            ButtonText = ModResources.TeleportButtonText
        };
    }
    internal override void SetButtonCooldowns()
    {
        TeleportButton.MaxTimer = Cooldown;
    }

    internal override void Clear()
    {
        players = new List<Teleporter>();
    }
}