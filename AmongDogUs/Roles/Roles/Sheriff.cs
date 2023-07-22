namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Sheriff : RoleBase<Sheriff>
{
    internal static CustomButton SheriffKillButton;
    internal static TMP_Text SheriffNumShotsText;
    public Sheriff()
    {
        RoleType = roleType = RoleType.Sheriff;
        ReamingShots = MaxShots;
    }

    internal static PlayerControl currentTarget;
    internal static float Cooldown { get { return CustomOptionsHolder.SheriffCooldowns.GetFloat(); } }
    internal static int MaxShots { get { return Mathf.RoundToInt(CustomOptionsHolder.SheriffMaxShots.GetFloat()); } }
    internal static bool CanKillNeutrals { get { return CustomOptionsHolder.SheriffCanKillNeutral.GetBool(); } }
    internal static bool MisfireKillsTarget { get { return CustomOptionsHolder.SheriffMisfireKillsTarget.GetBool(); } }
    internal static int ReamingShots = 1;

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.Sheriff) && MaxShots > 0)
        {
            currentTarget = PlayerControlPatch.SetTarget();
            PlayerControlPatch.SetPlayerOutline(currentTarget, SheriffYellow);
        }
    }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        SheriffKillButton = new(
            () =>
            {
                if (ReamingShots <= 0) return;
                MurderAttemptResult murderAttemptResult = Helpers.CheckMurderAttempt(PlayerControl.LocalPlayer, Sheriff.currentTarget);
                if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;
                if (murderAttemptResult == MurderAttemptResult.PerformKill)
                {
                    bool misfire = false;
                    byte targetId = currentTarget.PlayerId;
                    if (currentTarget.Data.Role.IsImpostor ||
                        (CanKillNeutrals && currentTarget.IsNeutral()) ||
                        (Madmate.CanDieToSheriffOrYakuza && currentTarget.IsRole(RoleType.Madmate)))
                    {
                        targetId = currentTarget.PlayerId;
                        misfire = false;
                    }
                    else
                    {
                        targetId = PlayerControl.LocalPlayer.PlayerId;
                        misfire = true;
                    }
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.SheriffKill, SendOption.Reliable, -1);
                    killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                    killWriter.Write(targetId);
                    killWriter.Write(misfire);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.SheriffKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                }
                SheriffKillButton.Timer = SheriffKillButton.MaxTimer;
                currentTarget = null;
            },
            () => { return PlayerControl.LocalPlayer.IsRole(RoleType.Sheriff) && ReamingShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                if (SheriffNumShotsText != null)
                {
                    if (ReamingShots > 0) SheriffNumShotsText.text = string.Format(ModResources.ReamingShots, ReamingShots);
                    else SheriffNumShotsText.text = "";
                }
                return currentTarget && PlayerControl.LocalPlayer.CanMove;
            },
            () => { SheriffKillButton.Timer = SheriffKillButton.MaxTimer; },
            hm.KillButton.graphic.sprite,
            ButtonPositions.RightTop,
            hm,
            hm.KillButton,
            KeyCode.Q
        );
        SheriffNumShotsText = Object.Instantiate(SheriffKillButton.actionButton.cooldownTimerText, SheriffKillButton.actionButton.cooldownTimerText.transform.parent);
        SheriffNumShotsText.text = "";
        SheriffNumShotsText.enableWordWrapping = false;
        SheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
        SheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }
    internal override void SetButtonCooldowns()
    {
        SheriffKillButton.MaxTimer = Cooldown;
    }

    internal override void Clear()
    {
        players = new List<Sheriff>();
    }
}