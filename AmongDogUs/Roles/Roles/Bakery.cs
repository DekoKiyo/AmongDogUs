namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Bakery : RoleBase<Bakery>
{
    internal static float BombRate { get { return CustomOptionsHolder.BakeryBombRate.GetFloat(); } }

    public Bakery()
    {
        RoleType = roleType = RoleType.Bakery;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm) { }
    internal override void SetButtonCooldowns() { }

    internal override void Clear()
    {
        players = new List<Bakery>();
    }
}