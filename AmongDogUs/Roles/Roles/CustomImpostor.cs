namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class CustomImpostor : RoleBase<CustomImpostor>
{
    public CustomImpostor()
    {
        RoleType = roleType = RoleType.CustomImpostor;
    }

    internal static float KillCooldowns { get { return CustomOptionsHolder.CustomImpostorKillCooldown.GetFloat(); } }
    internal static bool CanUseVents { get { return CustomOptionsHolder.CustomImpostorCanUseVents.GetBool(); } }
    internal static bool CanSabotage { get { return CustomOptionsHolder.CustomImpostorCanSabotage.GetBool(); } }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.CustomImpostor)) player.SetKillTimerUnchecked(KillCooldowns);
    }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target)
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleType.CustomImpostor)) player.SetKillTimerUnchecked(KillCooldowns);
    }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm) { }
    internal override void SetButtonCooldowns() { }

    internal override void Clear()
    {
        players = new List<CustomImpostor>();
    }
}