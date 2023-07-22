namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class Lighter : RoleBase<Lighter>
{
    internal static CustomButton LighterButton;
    public Lighter()
    {
        RoleType = roleType = RoleType.Lighter;
    }

    internal static Sprite LighterButtonSprite;
    internal static float LighterModeLightsOnVision { get { return CustomOptionsHolder.LighterModeLightsOnVision.GetFloat(); } }
    internal static float LighterModeLightsOffVision { get { return CustomOptionsHolder.LighterModeLightsOffVision.GetFloat(); } }
    internal static float Cooldown { get { return CustomOptionsHolder.LighterCooldown.GetFloat(); } }
    internal static float Duration { get { return CustomOptionsHolder.LighterDuration.GetFloat(); } }
    internal static bool LightActive = false;

    internal static Sprite GetButtonSprite()
    {
        if (LighterButtonSprite) return LighterButtonSprite;
        LighterButtonSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.LighterLight, 115f);
        return LighterButtonSprite;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        LighterButton = new(
            () =>
            {
                LightActive = true;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.Lighter) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                LightActive = false;
                LighterButton.Timer = LighterButton.MaxTimer;
                LighterButton.IsEffectActive = false;
                LighterButton.actionButton.graphic.color = Palette.EnabledColor;
            },
            GetButtonSprite(),
            ButtonPositions.RightTop,
            hm,
            hm.UseButton,
            KeyCode.F,
            true,
            Duration,
            () =>
            {
                LightActive = false;
                LighterButton.Timer = LighterButton.MaxTimer;
            }
        )
        {
            ButtonText = ModResources.LighterText,
            EffectCancellable = true
        };
    }
    internal override void SetButtonCooldowns()
    {
        LighterButton.MaxTimer = Cooldown;
        LighterButton.EffectDuration = Duration;
    }

    internal override void Clear()
    {
        players = new List<Lighter>();
        LightActive = false;
    }
}