namespace AmongDogUs.Modules;

internal static class ModAssets
{
    private static readonly Assembly AudioAssets = Assembly.GetExecutingAssembly();
    private static readonly Assembly SpriteAssets = Assembly.GetExecutingAssembly();
    private static readonly Assembly ButtonAssets = Assembly.GetExecutingAssembly();
    private static readonly Assembly MapAssets = Assembly.GetExecutingAssembly();
    // private static readonly Assembly GodMiraHQAssets = Assembly.GetExecutingAssembly();

    internal static AudioClip JesterWinSound;
    internal static AudioClip EveryoneLoseSound;
    internal static AudioClip Bomb;
    internal static AudioClip Teleport;
    internal static AudioClip UIHover;
    internal static AudioClip UISelect;

    internal static Texture2D Arrow;
    internal static Texture2D DeadBodySprite;
    internal static Texture2D TabGeneral;
    internal static Texture2D TabCrew;
    internal static Texture2D TabImp;
    internal static Texture2D TabNeu;
    internal static Texture2D TabMod;
    internal static Texture2D TabOth;
    internal static Texture2D Soul;

    internal static Texture2D EngineerRepairButton;
    internal static Texture2D UnderTakerMoveButton;
    internal static Texture2D TeleporterTeleportButton;
    internal static Texture2D AltruistReviveButton;
    internal static Texture2D JackalSidekickButton;
    internal static Texture2D ArsonistDouseButton;
    internal static Texture2D ArsonistIgniteButton;
    internal static Texture2D LighterLight;
    internal static Texture2D MeetingButton;

    // internal static GameObject GodMiraHQ;
    // internal static GameObject NewDropShip;

    internal static void LoadAssets()
    {
        var AudioAssetsResource = AudioAssets.GetManifestResourceStream("AmongDogUs.Resources.audio");
        var AudioAssetsBundle = AssetBundle.LoadFromMemory(AudioAssetsResource.ReadFully());

        JesterWinSound = AudioAssetsBundle.LoadAsset<AudioClip>("JesterWin.wav").DontUnload();
        EveryoneLoseSound = AudioAssetsBundle.LoadAsset<AudioClip>("EveryoneLose.wav").DontUnload();
        Bomb = AudioAssetsBundle.LoadAsset<AudioClip>("Bomb.mp3").DontUnload();
        Teleport = AudioAssetsBundle.LoadAsset<AudioClip>("Teleport.mp3").DontUnload();
        UIHover = AudioAssetsBundle.LoadAsset<AudioClip>("UI_Hover.wav").DontUnload();
        UISelect = AudioAssetsBundle.LoadAsset<AudioClip>("UI_Select.wav").DontUnload();

        var SpriteAssetsResource = SpriteAssets.GetManifestResourceStream("AmongDogUs.Resources.sprite");
        var SpriteAssetsBundle = AssetBundle.LoadFromMemory(SpriteAssetsResource.ReadFully());

        Arrow = SpriteAssetsBundle.LoadAsset<Texture2D>("Arrow.png").DontUnload();
        DeadBodySprite = SpriteAssetsBundle.LoadAsset<Texture2D>("DeadBody.png").DontUnload();
        Soul = SpriteAssetsBundle.LoadAsset<Texture2D>("Soul.png").DontUnload();

        var ButtonAssetsResource = ButtonAssets.GetManifestResourceStream("AmongDogUs.Resources.button");
        var ButtonAssetsBundle = AssetBundle.LoadFromMemory(ButtonAssetsResource.ReadFully());

        TabGeneral = ButtonAssetsBundle.LoadAsset<Texture2D>("GeneralTab.png").DontUnload();
        TabCrew = ButtonAssetsBundle.LoadAsset<Texture2D>("CrewTab.png").DontUnload();
        TabImp = ButtonAssetsBundle.LoadAsset<Texture2D>("ImpTab.png").DontUnload();
        TabNeu = ButtonAssetsBundle.LoadAsset<Texture2D>("NeutralTab.png").DontUnload();
        TabMod = ButtonAssetsBundle.LoadAsset<Texture2D>("ModifierTab.png").DontUnload();
        TabOth = ButtonAssetsBundle.LoadAsset<Texture2D>("OtherTab.png").DontUnload();
        EngineerRepairButton = ButtonAssetsBundle.LoadAsset<Texture2D>("EngineerRepairButton.png").DontUnload();
        UnderTakerMoveButton = ButtonAssetsBundle.LoadAsset<Texture2D>("UnderTakerMoveButton.png").DontUnload();
        TeleporterTeleportButton = ButtonAssetsBundle.LoadAsset<Texture2D>("TeleporterTeleportButton.png").DontUnload();
        AltruistReviveButton = ButtonAssetsBundle.LoadAsset<Texture2D>("AltruistReviveButton.png").DontUnload();
        JackalSidekickButton = ButtonAssetsBundle.LoadAsset<Texture2D>("JackalSidekickButton.png").DontUnload();
        ArsonistDouseButton = ButtonAssetsBundle.LoadAsset<Texture2D>("ArsonistDouse.png").DontUnload();
        ArsonistIgniteButton = ButtonAssetsBundle.LoadAsset<Texture2D>("ArsonistIgnite.png").DontUnload();
        LighterLight = ButtonAssetsBundle.LoadAsset<Texture2D>("LighterButton.png").DontUnload();
        MeetingButton = ButtonAssetsBundle.LoadAsset<Texture2D>("MeetingButton.png").DontUnload();

        var AirShipAssetsResource = MapAssets.GetManifestResourceStream("AmongDogUs.Resources.airship");
        var AirShipAssetsBundle = AssetBundle.LoadFromMemory(AirShipAssetsResource.ReadFully());

        AirShipOption.OptionMenuPrefab = AirShipAssetsBundle.LoadAsset<GameObject>("MapOptionMenu.prefab").DontUnload();

        // var GodMiraHQAssetsResource = GodMiraHQAssets.GetManifestResourceStream("UltimateMods.GodMiraHQ.Resources.godmirahq");
        // var GodMiraHQAssetsBundle = AssetBundle.LoadFromMemory(GodMiraHQAssetsResource.ReadFully());

        // GodMiraHQ = GodMiraHQAssetsBundle.LoadAsset<GameObject>("GodMiraHQ.prefab").DontUnload();
        // NewDropShip = GodMiraHQAssetsBundle.LoadAsset<GameObject>("DropShip.prefab").DontUnload();

        AudioAssetsBundle.Unload(false);
        SpriteAssetsBundle.Unload(false);
        ButtonAssetsBundle.Unload(false);
        AirShipAssetsBundle.Unload(false);
        // GodMiraHQAssetsBundle.Unload(false);
    }
}