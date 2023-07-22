namespace AmongDogUs.Roles;

[HarmonyPatch]
internal class EvilHacker : RoleBase<EvilHacker>
{
    internal static CustomButton EvilHackerAdminButton;
    public EvilHacker()
    {
        RoleType = roleType = RoleType.EvilHacker;
    }

    internal static bool CanHasBetterAdmin { get { return CustomOptionsHolder.EvilHackerCanHasBetterAdmin.GetBool(); } }

    internal static Sprite GetButtonSprite()
    {
        byte mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        UseButtonSettings button = mapId switch
        {
            1 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton],
            4 => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton],
            _ => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton],
        };
        return button.Image;
    }

    internal override void OnMeetingStart() { }
    internal override void OnMeetingEnd() { }
    internal override void FixedUpdate() { }
    internal override void OnKill(PlayerControl target) { }
    internal override void OnDeath(PlayerControl killer = null) { }
    internal override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    internal override void MakeButtons(HudManager hm)
    {
        EvilHackerAdminButton = new(
            () =>
            {
                if (!CustomOptionsHolder.EvilHackerCanMoveEvenIfUsesAdmin.GetBool()) PlayerControl.LocalPlayer.NetTransform.Halt();
                FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new()
                {
                    Mode = MapOptions.Modes.CountOverlay,
                    AllowMovementWhileMapOpen = CustomOptionsHolder.EvilHackerCanMoveEvenIfUsesAdmin.GetBool(),
                    IncludeDeadBodies = true
                });
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleType.EvilHacker) && PlayerControl.LocalPlayer.IsAlive();
            },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () => { EvilHackerAdminButton.MaxTimer = 0f; },
            GetButtonSprite(),
            ButtonPositions.LeftTop,
            hm,
            hm.KillButton,
            KeyCode.F
        )
        {
            ButtonText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin)
        };
    }
    internal override void SetButtonCooldowns()
    {
        EvilHackerAdminButton.MaxTimer = EvilHackerAdminButton.Timer = 0f;
    }

    internal override void Clear()
    {
        players = new List<EvilHacker>();
    }
}