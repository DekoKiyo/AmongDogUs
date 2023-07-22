namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Opportunist : ModifierBase<Opportunist>
{
    internal override string ModifierPostfix() { return "OP"; }

    public Opportunist()
    {
        ModType = modType = ModifierType.Opportunist;
    }

    internal static List<PlayerControl> Candidates
    {
        get
        {
            List<PlayerControl> validPlayers = new();

            foreach (var player in PlayerControl.AllPlayerControls) if (!player.HasModifier(ModifierType.Opportunist)) validPlayers.Add(player);

            return validPlayers;
        }
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
        players = new();
    }
}