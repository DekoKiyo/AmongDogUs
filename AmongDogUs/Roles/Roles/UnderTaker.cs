namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class UnderTaker : RoleBase<UnderTaker>
{
    internal static CustomButton UnderTakerButton;
    public UnderTaker()
    {
        RoleType = roleType = RoleType.UnderTaker;
    }

    internal static Sprite UnderTakerButtonSprite;

    internal static float KillCooldown { get { return CustomOptionsHolder.UnderTakerKillCooldown.GetFloat(); } }
    internal static float MoveCooldown { get { return CustomOptionsHolder.UnderTakerDuration.GetFloat(); } }
    internal static bool HasDuration { get { return CustomOptionsHolder.UnderTakerHasDuration.GetBool(); } }
    internal static float Duration { get { return CustomOptionsHolder.UnderTakerDuration.GetFloat(); } }
    internal static float SpeedDown { get { return CustomOptionsHolder.UnderTakerDraggingSpeed.GetFloat(); } }
    internal static bool CanDumpVent { get { return CustomOptionsHolder.UnderTakerCanDumpBodyVents.GetBool(); } }

    internal static bool DraggingBody = false;
    internal static byte BodyId = 0;

    internal static Sprite GetButtonSprite()
    {
        if (UnderTakerButtonSprite) return UnderTakerButtonSprite;
        UnderTakerButtonSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.UnderTakerMoveButton, 115f);
        return UnderTakerButtonSprite;
    }

    internal static void UnderTakerResetValuesAtDead()
    {
        DraggingBody = false;
        BodyId = 0;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.UnderTaker)) player.SetKillTimerUnchecked(KillCooldown);
    }
    internal override void FixedUpdate()
    {
        if (DraggingBody)
        {
            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == BodyId)
                {
                    foreach (var underTaker in AllPlayers)
                    {
                        var currentPosition = underTaker.GetTruePosition();
                        var velocity = underTaker.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
                        velocity *= SpeedDown / 100f;
                        var newPos = underTaker.GetTruePosition() - (velocity / 3) + new Vector2(0.15f, 0.25f) + array[i].myCollider.offset;
                        if (!PhysicsHelpers.AnythingBetween(
                            currentPosition,
                            newPos,
                            Constants.ShipAndObjectsMask,
                            false
                        ))
                        {
                            if (GameManager.Instance.LogicOptions.currentGameOptions.GetByte(ByteOptionNames.MapId) == 5)
                            {
                                array[i].transform.position = newPos;
                                array[i].transform.position += new Vector3(0, 0, -0.5f);
                            }
                            else array[i].transform.position = newPos;
                        }
                    }
                }
            }
        }
    }
    internal override void OnKill(PlayerControl target)
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.UnderTaker)) player.SetKillTimerUnchecked(KillCooldown);
    }
    internal override void OnDeath(PlayerControl killer = null)
    {
        if (DraggingBody) UnderTakerResetValuesAtDead();
    }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        UnderTakerButton = new(
            () =>
            {
                if (DraggingBody)
                {
                    if (HasDuration && UnderTakerButton.IsEffectActive)
                    {
                        UnderTakerButton.Timer = 0f;
                        return;
                    }
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.DragPlaceBody, SendOption.Reliable, -1);
                    writer.Write(BodyId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.DragPlaceBody(BodyId);
                    foreach (var underTaker in AllPlayers) underTaker.killTimer = KillCooldown;
                    UnderTakerButton.Timer = UnderTakerButton.MaxTimer = MoveCooldown;
                }
                else
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.DragPlaceBody, SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.DragPlaceBody(playerInfo.PlayerId);
                                    break;
                                }
                            }
                        }
                    }
                }
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.UnderTaker) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () =>
            {
                if (DraggingBody) UnderTakerButton.ButtonText = ModResources.UnderTakerDropText;
                else UnderTakerButton.ButtonText = ModResources.UnderTakerDragText;
                bool canDrag = false;
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask)) if (collider2D.tag == "DeadBody") canDrag = true;
                return canDrag && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                UnderTakerButton.Timer = UnderTakerButton.MaxTimer = MoveCooldown;
                UnderTakerResetValuesAtDead();
            },
            GetButtonSprite(),
            ButtonPositions.LeftTop,
            hm,
            hm.KillButton,
            KeyCode.F,
            HasDuration,
            Duration,
            () =>
            {
                if (DraggingBody)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.DragPlaceBody, SendOption.Reliable, -1);
                    writer.Write(BodyId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.DragPlaceBody(BodyId);
                }
                else
                {
                    DraggingBody = false;
                    BodyId = 0;
                    foreach (var underTaker in AllPlayers) underTaker.killTimer = KillCooldown;
                }
                UnderTakerButton.Timer = UnderTakerButton.MaxTimer = MoveCooldown;
            }
        )
        {
            ButtonText = ModResources.UnderTakerDragText,
            EffectCancellable = true
        };
    }
    internal override void SetButtonCooldowns()
    {
        UnderTakerButton.MaxTimer = MoveCooldown;
        UnderTakerButton.EffectDuration = Duration;
    }

    internal static void OnEnterVent()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.UnderTaker) && CanDumpVent && DraggingBody)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.CleanBody, SendOption.Reliable, -1);
            writer.Write(BodyId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.CleanBody(BodyId);
            DraggingBody = false;
            if (HasDuration && UnderTakerButton.IsEffectActive)
            {
                UnderTakerButton.Timer = 0f;
                return;
            }
        }
    }

    internal override void Clear()
    {
        players = new List<UnderTaker>();
    }
}