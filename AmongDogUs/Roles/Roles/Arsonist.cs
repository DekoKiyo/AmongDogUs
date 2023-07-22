namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Arsonist : RoleBase<Arsonist>
{
    internal static CustomButton ArsonistButton;
    internal static CustomButton IgniteButton;
    public Arsonist()
    {
        RoleType = roleType = RoleType.Arsonist;
    }

    internal static PlayerControl CurrentTarget;
    internal static PlayerControl DouseTarget;
    internal static List<PlayerControl> DousedPlayers = new();

    internal static float Cooldown { get { return CustomOptionsHolder.ArsonistCooldown.GetFloat(); } }
    internal static float Duration { get { return CustomOptionsHolder.ArsonistDuration.GetFloat(); } }
    internal static bool DousedEveryone = false;

    private static Sprite DouseSprite;
    private static Sprite IgniteSprite;
    internal static Sprite GetDouseSprite()
    {
        if (DouseSprite) return DouseSprite;
        DouseSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.ArsonistDouseButton, 115f);
        return DouseSprite;
    }
    internal static Sprite GetIgniteSprite()
    {
        if (IgniteSprite) return IgniteSprite;
        IgniteSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.ArsonistIgniteButton, 115f);
        return IgniteSprite;
    }

    internal static bool DousedEveryoneAlive()
    {
        return PlayerControl.AllPlayerControls.ToArray().All(x =>
        {
            return x == x.IsRole(RoleType.Arsonist) || x.Data.IsDead || x.Data.Disconnected || DousedPlayers.Any(y => y.PlayerId == x.PlayerId);
        });
    }

    internal static void UpdateStatus()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.Arsonist))
        {
            DousedEveryone = DousedEveryoneAlive();
        }
    }

    internal static void UpdateIcons()
    {
        foreach (PoolablePlayer pp in ModMapOptions.PlayerIcons.Values)
        {
            pp.gameObject.SetActive(false);
        }

        if (PlayerControl.LocalPlayer.IsRole(RoleType.Arsonist))
        {
            int visibleCounter = 0;
            Vector3 bottomLeft = new(-FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.x, -FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.z);
            bottomLeft += new Vector3(0.2f, 0.25f, 0);

            foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
            {
                if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                if (!ModMapOptions.PlayerIcons.ContainsKey(p.PlayerId)) continue;

                if (p.Data.IsDead || p.Data.Disconnected)
                {
                    ModMapOptions.PlayerIcons[p.PlayerId].gameObject.SetActive(false);
                }
                else
                {
                    ModMapOptions.PlayerIcons[p.PlayerId].gameObject.SetActive(true);
                    ModMapOptions.PlayerIcons[p.PlayerId].transform.localScale = Vector3.one * 0.25f;
                    ModMapOptions.PlayerIcons[p.PlayerId].transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.45f;
                    visibleCounter++;
                }
                bool isDoused = DousedPlayers.Any(x => x.PlayerId == p.PlayerId);
                ModMapOptions.PlayerIcons[p.PlayerId].SetSemiTransparent(!isDoused);
            }
        }
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd()
    {
        UpdateIcons();
    }
    internal override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.Arsonist))
        {
            List<PlayerControl> UnTargetables;
            if (DouseTarget != null) UnTargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != DouseTarget.PlayerId).ToList();
            else UnTargetables = DousedPlayers;
            CurrentTarget = PlayerControlPatch.SetTarget(unTargetablePlayers: UnTargetables);
            if (CurrentTarget != null) PlayerControlPatch.SetPlayerOutline(CurrentTarget, ArsonistOrange);
        }
    }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        ArsonistButton = new(
            () =>
            {
                if (CurrentTarget != null) DouseTarget = CurrentTarget;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.Arsonist) && !DousedEveryone && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () =>
            {
                if (DousedEveryone) ArsonistButton.ButtonText = ModResources.IgniteText;
                else ArsonistButton.ButtonText = ModResources.DouseText;
                if (ArsonistButton.IsEffectActive && DouseTarget != CurrentTarget)
                {
                    DouseTarget = null;
                    ArsonistButton.Timer = 0f;
                    ArsonistButton.IsEffectActive = false;
                }
                return PlayerControl.LocalPlayer.CanMove && CurrentTarget != null;
            },
            () =>
            {
                ArsonistButton.Timer = ArsonistButton.MaxTimer;
                ArsonistButton.IsEffectActive = false;
                DouseTarget = null;
                UpdateStatus();
            },
            GetDouseSprite(),
            ButtonPositions.RightTop,
            hm,
            hm.KillButton,
            KeyCode.F,
            true,
            Duration,
            () =>
            {
                if (DouseTarget != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.ArsonistDouse, SendOption.Reliable, -1);
                    writer.Write(DouseTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ArsonistDouse(DouseTarget.PlayerId);
                }
                DouseTarget = null;
                UpdateStatus();
                ArsonistButton.Timer = DousedEveryone ? 0 : ArsonistButton.MaxTimer;
                foreach (PlayerControl p in DousedPlayers)
                {
                    if (ModMapOptions.PlayerIcons.ContainsKey(p.PlayerId))
                    {
                        ModMapOptions.PlayerIcons[p.PlayerId].SetSemiTransparent(false);
                    }
                }
            }
        )
        {
            ButtonText = ModResources.DouseText
        };

        IgniteButton = new(
            () =>
            {
                if (DousedEveryone)
                {
                    MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.ArsonistWin, SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                    RPCProcedure.ArsonistWin();
                }
            },
            () => { return PlayerControl.LocalPlayer.IsRole(RoleType.Arsonist) && !PlayerControl.LocalPlayer.Data.IsDead && DousedEveryone; },
            () => { return PlayerControl.LocalPlayer.CanMove && DousedEveryone; },
            () => { },
            GetIgniteSprite(),
            ButtonPositions.RightTop,
            hm,
            hm.KillButton,
            KeyCode.F
        )
        {
            ButtonText = ModResources.IgniteText
        };
    }
    internal override void SetButtonCooldowns()
    {
        ArsonistButton.MaxTimer = Cooldown;
        IgniteButton.MaxTimer = IgniteButton.Timer = 0f;
    }

    internal override void Clear()
    {
        players = new List<Arsonist>();

        CurrentTarget = null;
        DouseTarget = null;
        DousedEveryone = false;
        DousedPlayers = new();
        foreach (PoolablePlayer p in ModMapOptions.PlayerIcons.Values) if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
    }
}