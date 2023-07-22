namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Altruist : RoleBase<Altruist>
{
    internal static Sprite AltruistButtonSprite;
    internal static bool Started = false;
    internal static bool Ended = false;
    internal static DeadBody Target;

    internal static CustomButton AltruistButton;
    internal static float Duration { get { return CustomOptionsHolder.AltruistDuration.GetFloat(); } }

    public Altruist()
    {
        RoleType = roleType = RoleType.Altruist;
    }

    internal static Sprite GetButtonSprite()
    {
        if (AltruistButtonSprite) return AltruistButtonSprite;
        AltruistButtonSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.AltruistReviveButton, 115f);
        return AltruistButtonSprite;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate()
    {
        var TruePosition = PlayerControl.LocalPlayer.GetTruePosition();
        var MaxDistance = GameOptionsData.KillDistances[GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance)];
        var flag = (GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.GhostsDoTasks) || !PlayerControl.LocalPlayer.Data.IsDead) &&
                    (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) &&
                    PlayerControl.LocalPlayer.CanMove;
        var OverlapCircle = Physics2D.OverlapCircleAll(TruePosition, MaxDistance, LayerMask.GetMask(new[] { "Players", "Ghost" }));
        var ClosestDistance = float.MaxValue;

        foreach (var collider2D in OverlapCircle)
        {
            if (!flag || PlayerControl.LocalPlayer.Data.IsDead || collider2D.tag != "DeadBody" || Started) continue;
            Target = collider2D.GetComponent<DeadBody>();

            if (!(Vector2.Distance(TruePosition, Target.TruePosition) <= MaxDistance)) continue;

            var Distance = Vector2.Distance(TruePosition, Target.TruePosition);
            if (!(Distance < ClosestDistance)) continue;
            ClosestDistance = Distance;
        }
    }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

    internal override void MakeButtons(HudManager hm)
    {
        AltruistButton = new(
            () =>
            {
                MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.AltruistKill, SendOption.Reliable, -1);
                killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                RPCProcedure.AltruistKill(PlayerControl.LocalPlayer.Data.PlayerId);
                Started = true;
            },
            () => { return PlayerControl.LocalPlayer.IsRole(RoleType.Altruist) && !Ended; },
            () =>
            {
                bool CanRevive = false;
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask)) if (collider2D.tag == "DeadBody") CanRevive = true;
                return CanRevive && PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            GetButtonSprite(),
            ButtonPositions.RightTop,
            hm,
            hm.KillButton,
            KeyCode.F,
            true,
            Duration,
            () =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.AltruistRevive, SendOption.Reliable, -1);
                writer.Write(Target.ParentId);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.AltruistRevive(Target.ParentId, PlayerControl.LocalPlayer.PlayerId);
                Ended = true;
            }
        )
        {
            ButtonText = ModResources.AltruistReviveText,
            EffectCancellable = false
        };
    }

    internal override void SetButtonCooldowns()
    {
        AltruistButton.Timer = AltruistButton.MaxTimer = 0f;
    }

    internal override void Clear()
    {
        players = new List<Altruist>();

        Started = false;
        Ended = false;
    }
}