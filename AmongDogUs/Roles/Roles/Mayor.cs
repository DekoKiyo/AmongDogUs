namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Mayor : RoleBase<Mayor>
{
    internal static CustomButton MayorMeetingButton;
    internal static TMP_Text MayorMeetingButtonText;
    public Mayor()
    {
        RoleType = roleType = RoleType.Mayor;
        ReamingCount = MaxButton;
    }

    internal static Sprite MayorMeetingButtonSprite;
    internal static int NumVotes { get { return Mathf.RoundToInt(CustomOptionsHolder.MayorNumVotes.GetFloat()); } }
    internal static bool HasMeetingButton { get { return CustomOptionsHolder.MayorMeetingButton.GetBool(); } }
    internal static int MaxButton { get { return Mathf.RoundToInt(CustomOptionsHolder.MayorNumMeetingButton.GetFloat()); } }
    internal static int ReamingCount = 1;

    internal static Sprite GetButtonSprite()
    {
        if (MayorMeetingButtonSprite) return MayorMeetingButtonSprite;
        MayorMeetingButtonSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.MeetingButton, 550f);
        return MayorMeetingButtonSprite;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        MayorMeetingButton = new(
            () =>
            {
                if (ReamingCount <= 0) return;
                ReamingCount--;

                PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UncheckedCmdReportDeadBody, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.UncheckedCmdReportDeadBody(PlayerControl.LocalPlayer.PlayerId, byte.MinValue);

                MayorMeetingButton.Timer = MayorMeetingButton.MaxTimer;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.Mayor) && !PlayerControl.LocalPlayer.Data.IsDead && HasMeetingButton && ReamingCount > 0;
            },
            () =>
            {
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    if ((task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                    || (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)) sabotageActive = true;

                if (MayorMeetingButtonText != null)
                {
                    if (ReamingCount > 0) MayorMeetingButtonText.text = string.Format(ModResources.ReamingCount, ReamingCount);
                    else MayorMeetingButtonText.text = "";
                }

                return !sabotageActive && PlayerControl.LocalPlayer.CanMove;
            },
            () => { MayorMeetingButton.Timer = MayorMeetingButton.MaxTimer; },
            GetButtonSprite(),
            ButtonPositions.RightTop,
            hm,
            hm.UseButton,
            KeyCode.F
        )
        {
            ButtonText = ModResources.MayorMeetingButtonText
        };
        MayorMeetingButtonText = Object.Instantiate(MayorMeetingButton.actionButton.cooldownTimerText, MayorMeetingButton.actionButton.cooldownTimerText.transform.parent);
        MayorMeetingButtonText.text = "";
        MayorMeetingButtonText.enableWordWrapping = false;
        MayorMeetingButtonText.transform.localScale = Vector3.one * 0.5f;
        MayorMeetingButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }
    internal override void SetButtonCooldowns()
    {
        MayorMeetingButton.MaxTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.EmergencyCooldown);
    }

    internal override void Clear()
    {
        players = new List<Mayor>();
    }
}