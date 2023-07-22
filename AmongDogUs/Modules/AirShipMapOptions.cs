// Some Source Code from TORGMH

namespace AmongDogUs.Modules;

internal sealed class AirShipOption : MonoBehaviour
{
    internal static GameObject Prefab;
    private static GameObject OptionMenu;
    internal static GameObject OptionMenuPrefab;
    private Button CloseButton;
    private Button OpAirShipElecDoorButton;
    private static TMP_Dropdown Dropdown;
    private static TMP_Dropdown TimingDropdown;
    internal static int AirShipElecDoorOptData = 0;
    internal static int AirShipElecDoorTiming = 0;
    internal static int OptionNum = 0;

    private Button ElecDoorTopLeftVert;
    private Button ElecDoorTopLeftHort;
    private Button ElecDoorBottomHort;
    private Button ElecDoorTopCenterHort;
    private Button ElecDoorLeftVert;
    private Button ElecDoorRightVert;
    private Button ElecDoorTopRightVert;
    private Button ElecDoorTopRightHort;
    private Button ElecDoorBottomRightVert;
    private Button ElecDoorBottomRightHort;
    private Button ElecDoorLeftDoorTop;
    private Button ElecDoorLeftDoorBottom;

    private TMP_Text ElecMapOptLabel;
    private TMP_Text ChangeTimingLabel;

    internal void Awake()
    {
        if (Prefab == null)
        {
            Prefab = gameObject;
            gameObject.SetActive(false);
            return;
        }
        else gameObject.SetActive(true);

        PlayerControl.LocalPlayer.moveable = false;
        PlayerControl.LocalPlayer.NetTransform.Halt();

        ShowAndLoadSettings();
    }

    internal void FixedUpdate()
    {
        PlayerControl.LocalPlayer.moveable = false;
        if (Input.GetKey(KeyCode.Escape)) Close();
    }

    internal void OnEnable()
    {
        enabled = true;
    }

    internal static void OnDisable() { }

    internal static void OnDestroy()
    {
        PlayerControl.LocalPlayer.moveable = true;
    }

    private void ShowAndLoadSettings()
    {
        if (OptionMenu) Destroy(OptionMenu);
        OptionMenu = Instantiate(OptionMenuPrefab, transform);
        OptionMenu.SetActive(true);

        var ElecDoorMap = OptionMenu.GetComponentsInChildren<Image>().FirstOrDefault(x => x.name == "AirShipElecMap");
        ElecDoorMap.enabled = AirShipElecDoorOptData is 3;

        var buttons = OptionMenu.GetComponentsInChildren<Button>();

        CloseButton = buttons.FirstOrDefault(x => x.name == "CloseButton");
        CloseButton.onClick = new();
        CloseButton.onClick.AddListener((UnityAction)Close);

        // (オプション)AirShipドア開閉
        OpAirShipElecDoorButton = buttons.FirstOrDefault(x => x.name == "ElectricalDoor");
        OpAirShipElecDoorButton.onClick = new();
        OpAirShipElecDoorButton.onClick.AddListener((UnityAction)delegate
        {
            ShowAndLoadSettings();
            OptionNum = 0;
        });
        OpAirShipElecDoorButton.GetComponentInChildren<Text>().text = ModResources.AirShipElecDoorOption;

        // (オプション)AirShipベント
        // OpAirShipElecDoorButton = buttons.FirstOrDefault(x => x.name == "AirShipVents");
        // OpAirShipElecDoorButton.onClick = new();
        // OpAirShipElecDoorButton.onClick.AddListener((UnityAction)delegate
        // {
        //     ShowAndLoadSettings();
        //     OptionNum = 1;
        // });
        // OpAirShipElecDoorButton.GetComponentInChildren<Text>().text = LocalizationManager.GetString(TransKey.AirShipElecDoorOption);

        // ElecDoor Button
        ElecDoorTopLeftVert = buttons.FirstOrDefault(x => x.name == "ElecDoorTopLeftVert");
        ElecDoorTopLeftVert.onClick = new();
        ElecDoorTopLeftVert.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.TopLeftVert = !AirShipPatch.TopLeftVert;
            ElecDoorTopLeftVert.GetComponent<Image>().color = AirShipPatch.TopLeftVert ? TransparentWhite : Color.white;
            Main.TopLeftVert.Value = AirShipPatch.TopLeftVert;
        });
        ElecDoorTopLeftVert.GetComponent<Image>().color = AirShipPatch.TopLeftVert ? TransparentWhite : Color.white;
        ElecDoorTopLeftVert.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorTopLeftHort = buttons.FirstOrDefault(x => x.name == "ElecDoorTopLeftHort");
        ElecDoorTopLeftHort.onClick = new();
        ElecDoorTopLeftHort.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.TopLeftHort = !AirShipPatch.TopLeftHort;
            ElecDoorTopLeftHort.GetComponent<Image>().color = AirShipPatch.TopLeftHort ? TransparentWhite : Color.white;
            Main.TopLeftHort.Value = AirShipPatch.TopLeftHort;
        });
        ElecDoorTopLeftHort.GetComponent<Image>().color = AirShipPatch.TopLeftHort ? TransparentWhite : Color.white;
        ElecDoorTopLeftHort.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorBottomHort = buttons.FirstOrDefault(x => x.name == "ElecDoorBottomHort");
        ElecDoorBottomHort.onClick = new();
        ElecDoorBottomHort.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.BottomHort = !AirShipPatch.BottomHort;
            ElecDoorBottomHort.GetComponent<Image>().color = AirShipPatch.BottomHort ? TransparentWhite : Color.white;
            Main.BottomHort.Value = AirShipPatch.BottomHort;
        });
        ElecDoorBottomHort.GetComponent<Image>().color = AirShipPatch.BottomHort ? TransparentWhite : Color.white;
        ElecDoorBottomHort.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorTopCenterHort = buttons.FirstOrDefault(x => x.name == "ElecDoorTopCenterHort");
        ElecDoorTopCenterHort.onClick = new();
        ElecDoorTopCenterHort.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.TopCenterHort = !AirShipPatch.TopCenterHort;
            ElecDoorTopCenterHort.GetComponent<Image>().color = AirShipPatch.TopCenterHort ? TransparentWhite : Color.white;
            Main.TopCenterHort.Value = AirShipPatch.TopCenterHort;
        });
        ElecDoorTopCenterHort.GetComponent<Image>().color = AirShipPatch.TopCenterHort ? TransparentWhite : Color.white;
        ElecDoorTopCenterHort.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorLeftVert = buttons.FirstOrDefault(x => x.name == "ElecDoorLeftVert");
        ElecDoorLeftVert.onClick = new();
        ElecDoorLeftVert.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.LeftVert = !AirShipPatch.LeftVert;
            ElecDoorLeftVert.GetComponent<Image>().color = AirShipPatch.LeftVert ? TransparentWhite : Color.white;
            Main.LeftVert.Value = AirShipPatch.LeftVert;
        });
        ElecDoorLeftVert.GetComponent<Image>().color = AirShipPatch.LeftVert ? TransparentWhite : Color.white;
        ElecDoorLeftVert.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorRightVert = buttons.FirstOrDefault(x => x.name == "ElecDoorRightVert");
        ElecDoorRightVert.onClick = new();
        ElecDoorRightVert.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.RightVert = !AirShipPatch.RightVert;
            ElecDoorRightVert.GetComponent<Image>().color = AirShipPatch.RightVert ? TransparentWhite : Color.white;
            Main.RightVert.Value = AirShipPatch.RightVert;
        });
        ElecDoorRightVert.GetComponent<Image>().color = AirShipPatch.RightVert ? TransparentWhite : Color.white;
        ElecDoorRightVert.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorTopRightVert = buttons.FirstOrDefault(x => x.name == "ElecDoorTopRightVert");
        ElecDoorTopRightVert.onClick = new();
        ElecDoorTopRightVert.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.TopRightVert = !AirShipPatch.TopRightVert;
            ElecDoorTopRightVert.GetComponent<Image>().color = AirShipPatch.TopRightVert ? TransparentWhite : Color.white;
            Main.TopRightVert.Value = AirShipPatch.TopRightVert;
        });
        ElecDoorTopRightVert.GetComponent<Image>().color = AirShipPatch.TopRightVert ? TransparentWhite : Color.white;
        ElecDoorTopRightVert.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorTopRightHort = buttons.FirstOrDefault(x => x.name == "ElecDoorTopRightHort");
        ElecDoorTopRightHort.onClick = new();
        ElecDoorTopRightHort.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.TopRightHort = !AirShipPatch.TopRightHort;
            ElecDoorTopRightHort.GetComponent<Image>().color = AirShipPatch.TopRightHort ? TransparentWhite : Color.white;
            Main.TopRightHort.Value = AirShipPatch.TopRightHort;
        });
        ElecDoorTopRightHort.GetComponent<Image>().color = AirShipPatch.TopRightHort ? TransparentWhite : Color.white;
        ElecDoorTopRightHort.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorBottomRightVert = buttons.FirstOrDefault(x => x.name == "ElecDoorBottomRightVert");
        ElecDoorBottomRightVert.onClick = new();
        ElecDoorBottomRightVert.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.BottomRightVert = !AirShipPatch.BottomRightVert;
            ElecDoorBottomRightVert.GetComponent<Image>().color = AirShipPatch.BottomRightVert ? TransparentWhite : Color.white;
            Main.BottomRightVert.Value = AirShipPatch.BottomRightVert;
        });
        ElecDoorBottomRightVert.GetComponent<Image>().color = AirShipPatch.BottomRightVert ? TransparentWhite : Color.white;
        ElecDoorBottomRightVert.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorBottomRightHort = buttons.FirstOrDefault(x => x.name == "ElecDoorBottomRightHort");
        ElecDoorBottomRightHort.onClick = new();
        ElecDoorBottomRightHort.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.BottomRightHort = !AirShipPatch.BottomRightHort;
            ElecDoorBottomRightHort.GetComponent<Image>().color = AirShipPatch.BottomRightHort ? TransparentWhite : Color.white;
            Main.BottomRightHort.Value = AirShipPatch.BottomRightHort;
        });
        ElecDoorBottomRightHort.GetComponent<Image>().color = AirShipPatch.BottomRightHort ? TransparentWhite : Color.white;
        ElecDoorBottomRightHort.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorLeftDoorTop = buttons.FirstOrDefault(x => x.name == "ElecDoorLeftDoorTop");
        ElecDoorLeftDoorTop.onClick = new();
        ElecDoorLeftDoorTop.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.LeftDoorTop = !AirShipPatch.LeftDoorTop;
            ElecDoorLeftDoorTop.GetComponent<Image>().color = AirShipPatch.LeftDoorTop ? TransparentWhite : Color.white;
            Main.LeftDoorTop.Value = AirShipPatch.LeftDoorTop;
        });
        ElecDoorLeftDoorTop.GetComponent<Image>().color = AirShipPatch.LeftDoorTop ? TransparentWhite : Color.white;
        ElecDoorLeftDoorTop.transform.gameObject.active = ElecDoorMap.enabled;

        ElecDoorLeftDoorBottom = buttons.FirstOrDefault(x => x.name == "ElecDoorLeftDoorBottom");
        ElecDoorLeftDoorBottom.onClick = new();
        ElecDoorLeftDoorBottom.onClick.AddListener((UnityAction)delegate
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AirShipPatch.LeftDoorBottom = !AirShipPatch.LeftDoorBottom;
            ElecDoorLeftDoorBottom.GetComponent<Image>().color = AirShipPatch.LeftDoorBottom ? TransparentWhite : Color.white;
            Main.LeftDoorBottom.Value = AirShipPatch.LeftDoorBottom;
        });
        ElecDoorLeftDoorBottom.GetComponent<Image>().color = AirShipPatch.LeftDoorBottom ? TransparentWhite : Color.white;
        ElecDoorLeftDoorBottom.transform.gameObject.active = ElecDoorMap.enabled;

        var dropdown = OptionMenu.GetComponentsInChildren<TMP_Dropdown>();

        Dropdown = dropdown.FirstOrDefault(x => x.name == "ElecMapOpt");
        Dropdown.ClearOptions();
        var AllElecOptions = new Il2CppSystem.Collections.Generic.List<TMP_Dropdown.OptionData>();
        List<string> Options = new()
        {
            ModResources.DoorNormal,
            ModResources.DoorAllOpen,
            ModResources.DoorRandom,
            ModResources.DoorSelect,
        };

        foreach (var opt in Options)
        {
            var AirElecOp = new TMP_Dropdown.OptionData() { text = opt };
            AllElecOptions.Add(AirElecOp);
        }

        Dropdown.AddOptions(AllElecOptions);
        Dropdown.value = AirShipElecDoorOptData;
        Dropdown.RefreshShownValue();
        Dropdown.onValueChanged = new();
        Dropdown.onValueChanged.AddListener((UnityAction<int>)OnValueChanged);

        TimingDropdown = dropdown.FirstOrDefault(x => x.name == "ChangeTiming");
        TimingDropdown.ClearOptions();
        var TimingOptions = new Il2CppSystem.Collections.Generic.List<TMP_Dropdown.OptionData>();
        List<string> TimingOpt = new()
        {
            ModResources.OnMeetingEnd,
            ModResources.SomeoneKilled,
            ModResources.SomeoneInVent,
        };

        foreach (var opt in TimingOpt)
        {
            var TimingData = new TMP_Dropdown.OptionData() { text = opt };
            TimingOptions.Add(TimingData);
        }

        TimingDropdown.AddOptions(TimingOptions);
        TimingDropdown.value = AirShipElecDoorTiming;
        TimingDropdown.RefreshShownValue();
        TimingDropdown.onValueChanged = new();
        TimingDropdown.onValueChanged.AddListener((UnityAction<int>)OnTimingValueChanged);
        TimingDropdown.transform.gameObject.active = AirShipElecDoorOptData is 2;

        var TMPText = OptionMenu.GetComponentsInChildren<TMP_Text>();

        var Title = TMPText.FirstOrDefault(x => x.name == "TitleText(TMP)");
        Title.text = ModResources.AirShipOptionsTitle;

        ElecMapOptLabel = TMPText.FirstOrDefault(x => x.name == "ElecMapOptLabel");
        ElecMapOptLabel.text = ModResources.ElecMapOptLabel;
        ElecMapOptLabel.transform.gameObject.active = Dropdown.transform.gameObject.active;

        ChangeTimingLabel = TMPText.FirstOrDefault(x => x.name == "ChangeTimingLabel");
        ChangeTimingLabel.text = ModResources.ChangeTimingLabel;
        ChangeTimingLabel.transform.gameObject.active = TimingDropdown.transform.gameObject.active;
    }

    internal void OnValueChanged(int value)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        Main.AirShipDoorMode.Value = AirShipElecDoorOptData = value;
        ShowAndLoadSettings();
    }

    internal void OnTimingValueChanged(int value)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        Main.AirShipDoorChangeTiming.Value = AirShipElecDoorTiming = value;
        ShowAndLoadSettings();
    }

    internal void Close()
    {
        gameObject.SetActive(false);
        PlayerControl.LocalPlayer.moveable = true;
    }

    internal static void Load()
    {
        AirShipPatch.TopLeftVert = Main.TopLeftVert.Value;
        AirShipPatch.TopLeftHort = Main.TopLeftHort.Value;
        AirShipPatch.BottomHort = Main.BottomHort.Value;
        AirShipPatch.TopCenterHort = Main.TopCenterHort.Value;
        AirShipPatch.LeftVert = Main.LeftVert.Value;
        AirShipPatch.RightVert = Main.RightVert.Value;
        AirShipPatch.TopRightVert = Main.TopRightVert.Value;
        AirShipPatch.TopRightHort = Main.TopRightHort.Value;
        AirShipPatch.BottomRightVert = Main.BottomRightVert.Value;
        AirShipPatch.BottomRightHort = Main.BottomRightHort.Value;
        AirShipPatch.LeftDoorTop = Main.LeftDoorTop.Value;
        AirShipPatch.LeftDoorBottom = Main.LeftDoorBottom.Value;

        AirShipElecDoorOptData = Main.AirShipDoorMode.Value;
        AirShipElecDoorTiming = Main.AirShipDoorChangeTiming.Value;
    }

    [HarmonyPatch(typeof(LobbyBehaviour))]
    internal static class AirShipLobbyBehaviourPatch
    {
        internal static GameObject MapOptionDevice;
        internal static SpriteRenderer renderer;
        internal static OptionsConsole console;
        internal static BoxCollider2D collider2D;

        [HarmonyPatch(nameof(LobbyBehaviour.Start)), HarmonyPostfix]
        internal static void SetupLobbyObjects(LobbyBehaviour __instance)
        {
            var Lobby = GameObject.Find("Lobby(Clone)");

            MapOptionDevice = new("MapOptionDev");
            MapOptionDevice.transform.SetParent(Lobby.transform);
            MapOptionDevice.transform.localScale = new(0.5f, 0.5f, 0.5f);
            MapOptionDevice.transform.localPosition = new(0.01f, 3.35f, 0f);
            renderer = MapOptionDevice.AddComponent<SpriteRenderer>();
            renderer.sprite = EvilHacker.GetButtonSprite();

            GameObject obj = new("MapOptionMenu");
            obj.AddComponent<AirShipOption>();

            console = MapOptionDevice.AddComponent<OptionsConsole>();
            console.MenuPrefab = Prefab;
            console.CustomUseIcon = ImageNames.UseButton;
            console.HostOnly = false;

            collider2D = MapOptionDevice.AddComponent<BoxCollider2D>();
            collider2D.size = new(0.8f, 0.8f);
        }
    }
}