namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class ProEngineer : RoleBase<ProEngineer>
{
    internal static CustomButton EngineerRepairButton;
    internal static TMP_Text EngineerRepairButtonText;
    private static Sprite EngineerRepairButtonSprite;

    public ProEngineer()
    {
        RoleType = roleType = RoleType.ProEngineer;
        ReamingCounts = FixCount;
    }

    internal static bool CanFixSabo { get { return CustomOptionsHolder.EngineerCanFixSabo.GetBool(); } }
    internal static int FixCount { get { return Mathf.RoundToInt(CustomOptionsHolder.EngineerMaxFixCount.GetFloat()); } }
    internal static bool CanUseVents { get { return CustomOptionsHolder.EngineerCanUseVents.GetBool(); } }
    // internal static float VentCooldown { get { return CustomOptionsHolder.EngineerVentCooldown.getFloat(); } }
    internal static int ReamingCounts = 1;

    internal static Sprite GetFixButtonSprite()
    {
        if (EngineerRepairButtonSprite) return EngineerRepairButtonSprite;
        EngineerRepairButtonSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.EngineerRepairButton, 115f);
        return EngineerRepairButtonSprite;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        EngineerRepairButton = new(
            () =>
            {
                EngineerRepairButton.Timer = 0f;
                MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.EngineerUsedRepair, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                RPCProcedure.EngineerUsedRepair(PlayerControl.LocalPlayer.Data.PlayerId);
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                {
                    if (task.TaskType is TaskTypes.FixLights)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.EngineerFixLights, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.EngineerFixLights();
                    }
                    else if (task.TaskType is TaskTypes.RestoreOxy)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    }
                    else if (task.TaskType is TaskTypes.ResetReactor)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                    }
                    else if (task.TaskType is TaskTypes.ResetSeismic)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    }
                    else if (task.TaskType is TaskTypes.FixComms)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    }
                    else if (task.TaskType is TaskTypes.StopCharles)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                    }
                    else if (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)ECustomRPC.EngineerFixSubmergedOxygen, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.EngineerFixSubmergedOxygen();
                    }
                }
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.ProEngineer) && !PlayerControl.LocalPlayer.Data.IsDead && ReamingCounts > 0 && CanFixSabo;
            },
            () =>
            {
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles
                    || (SubmergedCompatibility.IsSubmerged && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask)) sabotageActive = true;
                if (EngineerRepairButtonText != null)
                {
                    if (ReamingCounts > 0) EngineerRepairButtonText.text = string.Format(ModResources.ReamingCount, ReamingCounts);
                    else EngineerRepairButtonText.text = "";
                }
                return sabotageActive && ReamingCounts > 0 && PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            GetFixButtonSprite(),
            ButtonPositions.RightTop,
            hm,
            hm.UseButton,
            KeyCode.F
        )
        {
            ButtonText = ModResources.EngineerRepairButtonText
        };
        EngineerRepairButtonText = Object.Instantiate(EngineerRepairButton.actionButton.cooldownTimerText, EngineerRepairButton.actionButton.cooldownTimerText.transform.parent); // TMP初期化
        EngineerRepairButtonText.text = "";
        EngineerRepairButtonText.enableWordWrapping = false;
        EngineerRepairButtonText.transform.localScale = Vector3.one * 0.5f;
        EngineerRepairButtonText.transform.localPosition += new Vector3(-0.05f, 0.5f, 0);
    }
    internal override void SetButtonCooldowns()
    {
        EngineerRepairButton.MaxTimer = EngineerRepairButton.Timer = 0f;
    }

    internal override void Clear()
    {
        players = new List<ProEngineer>();
    }
}