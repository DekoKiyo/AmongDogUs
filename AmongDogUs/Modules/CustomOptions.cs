namespace AmongDogUs.Modules;

internal enum CustomOptionType
{
    General,
    Impostor,
    Neutral,
    Crewmate,
    Modifiers,
    Other
}

internal class CustomOption
{
    internal static readonly string OPTION_OFF = ModResources.OptionOff;
    internal static readonly string OPTION_ON = ModResources.OptionOn;

    internal static List<CustomOption> options = new();
    internal static int Preset = 0;

    internal int id;
    internal string name;
    internal string format;
    internal object[] selections;

    internal int defaultSelection;
    internal ConfigEntry<int> entry;
    internal int selection;
    internal OptionBehaviour optionBehaviour;
    internal CustomOption parent;
    internal List<CustomOption> children;
    internal bool isHeader;
    internal bool isHidden;
    internal CustomOptionType type;
    internal Color Color;

    internal virtual bool Enabled { get { return CustomOptionsHolder.ActivateModRoles.GetBool() && GetBool(); } }

    // Option creation
    internal CustomOption() { }

    internal CustomOption(int id, CustomOptionType type, Color Color, string name, object[] selections, object defaultValue, string format, CustomOption parent, bool isHeader, bool isHidden)
    {
        Init(id, type, Color, name, selections, defaultValue, format, parent, isHeader, isHidden);
    }

    internal void Init(int id, CustomOptionType type, Color Color, string name, object[] selections, object defaultValue, string format, CustomOption parent, bool isHeader, bool isHidden)
    {
        this.id = id;
        this.name = name;
        this.format = format;
        this.selections = selections;
        int index = Array.IndexOf(selections, defaultValue);
        defaultSelection = index >= 0 ? index : 0;
        this.parent = parent;
        this.isHeader = isHeader;
        this.isHidden = isHidden;
        this.type = type;
        this.Color = Color;

        children = new List<CustomOption>();
        parent?.children.Add(this);

        selection = 0;
        if (id > 0)
        {
            entry = Main.Instance.Config.Bind($"Preset{Preset}", id.ToString(), defaultSelection);
            selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);

            if (options.Any(x => x.id == id))
            {
                Main.Instance.Log.LogWarning($"CustomOption id {id} is used in multiple places.");
            }
        }
        options.Add(this);
    }

    internal static CustomOption Create(int id, CustomOptionType type, Color Color, string name, string[] selections, string format, CustomOption parent = null, bool isHeader = false, bool isHidden = false)
    {
        return new(id, type, Color, name, selections, "", format, parent, isHeader, isHidden);
    }

    internal static CustomOption Create(int id, CustomOptionType type, Color Color, string name, float defaultValue, float min, float Max, float step, string format, CustomOption parent = null, bool isHeader = false, bool isHidden = false)
    {
        List<object> selections = new();
        for (float s = min; s <= Max; s += step) selections.Add(s);
        return new(id, type, Color, name, selections.ToArray(), defaultValue, format, parent, isHeader, isHidden);
    }

    internal static CustomOption Create(int id, CustomOptionType type, Color Color, string name, bool defaultValue, string format, CustomOption parent = null, bool isHeader = false, bool isHidden = false)
    {
        return new(id, type, Color, name, new string[] { OPTION_OFF, OPTION_ON }, defaultValue ? OPTION_ON : OPTION_OFF, format, parent, isHeader, isHidden);
    }

    // Static behaviour

    internal static void SwitchPreset(int newPreset)
    {
        Preset = newPreset;
        foreach (CustomOption option in options)
        {
            if (option.id <= 0) continue;

            option.entry = Main.Instance.Config.Bind($"Preset{Preset}", option.id.ToString(), option.defaultSelection);
            option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
            if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = option.selection;
                stringOption.ValueText.text = option.GetValueString();
            }
        }
    }

    internal static void ShareOptionSelections()
    {
        if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance!.AmHost == false && PlayerControl.LocalPlayer == null) return;

        var optionsList = new List<CustomOption>(options);
        while (optionsList.Any())
        {
            byte amount = (byte)Math.Min(optionsList.Count, 20);
            var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.ShareOptions, SendOption.Reliable, -1);
            writer.Write(amount);
            for (int i = 0; i < amount; i++)
            {
                var option = optionsList[0];
                optionsList.RemoveAt(0);
                writer.WritePacked((uint)option.id);
                writer.WritePacked(Convert.ToUInt32(option.selection));
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    // Getter

    internal virtual int GetSelection()
    {
        return selection;
    }

    internal virtual bool GetBool()
    {
        return selection > 0;
    }

    internal virtual float GetFloat()
    {
        return (float)selections[selection];
    }

    internal virtual string GetValueString()
    {
        string sel = selections[selection].ToString();
        if (!string.IsNullOrEmpty(format)) return string.Format(format, sel);
        return sel;
    }

    internal int GetQuantity()
    {
        return selection + 1;
    }

    internal virtual Color GetColor()
    {
        return Color;
    }

    // Option changes
    internal virtual void UpdateSelection(int newSelection)
    {
        selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
        if (optionBehaviour != null && optionBehaviour is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = selection;
            stringOption.ValueText.text = GetValueString();

            if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
            {
                if (id == 0) SwitchPreset(selection); // Switch Presets
                else if (entry != null) entry.Value = selection; // Save selection to config

                ShareOptionSelections(); // Share all selections
            }
        }
    }
}

internal class CustomRoleOption : CustomOption
{
    internal CustomOption countOption = null;
    internal bool roleEnabled = true;

    internal override bool Enabled { get { return CustomOptionsHolder.ActivateModRoles.GetBool() && roleEnabled && selection > 0; } }
    internal int Rate { get { return Enabled ? selection : 0; } }
    internal int Count { get { if (!Enabled) return 0; if (countOption != null) return Mathf.RoundToInt(countOption.GetFloat()); return 1; } }
    internal (int, int) Data { get { return (Rate, Count); } }
    internal CustomRoleOption(int id, CustomOptionType type, Color Color, string name, Color color, int Max = 15, bool roleEnabled = true) :
        base(id, type, Color, Helpers.ColorString(color, name), CustomOptionsHolder.TenRates, "", "", null, true, false)
    {
        this.roleEnabled = roleEnabled;

        if (Max <= 0 || !roleEnabled)
        {
            isHidden = true;
            this.roleEnabled = false;
        }

        if (Max > 1) countOption = Create(id + 10000, type, Color, "RoleNumAssigned", 1f, 1f, 15f, 1f, ModResources.FormatPlayer, this, false, isHidden);
    }
}

internal class CustomDualRoleOption : CustomRoleOption
{
    internal static List<CustomDualRoleOption> dualRoles = new();
    internal CustomOption roleImpChance = null;
    internal CustomOption roleAssignEqually = null;
    internal RoleId RoleId;

    internal int ImpChance { get { return roleImpChance.GetSelection(); } }

    internal bool AssignEqually { get { return roleAssignEqually.GetSelection() == 0; } }

    internal CustomDualRoleOption(int id, CustomOptionType type, Color Color, string name, Color color, RoleId RoleId, int Max = 15, bool roleEnabled = true) : base(id, type, Color, name, color, Max, roleEnabled)
    {
        roleAssignEqually = new CustomOption(id + 15001, type, Color, "roleAssignEqually", new string[] { OPTION_ON, OPTION_OFF }, OPTION_OFF, "", this, false, isHidden);
        roleImpChance = Create(id + 15000, type, Color, "roleImpChance", CustomOptionsHolder.TenRates, "", roleAssignEqually, false, isHidden);

        this.RoleId = RoleId;
        dualRoles.Add(this);
    }
}

internal class CustomTasksOption : CustomOption
{
    internal CustomOption commonTasksOption = null;
    internal CustomOption longTasksOption = null;
    internal CustomOption shortTasksOption = null;

    internal int CommonTasks { get { return Mathf.RoundToInt(commonTasksOption.GetSelection()); } }
    internal int LongTasks { get { return Mathf.RoundToInt(longTasksOption.GetSelection()); } }
    internal int ShortTasks { get { return Mathf.RoundToInt(shortTasksOption.GetSelection()); } }

    internal List<byte> GenerateTasks()
    {
        return Helpers.GenerateTasks(CommonTasks, ShortTasks, LongTasks);
    }

    internal CustomTasksOption(int id, CustomOptionType type, Color Color, int commonDef, int longDef, int shortDef, CustomOption parent = null)
    {
        commonTasksOption = Create(id + 20000, type, Color, "NumCommonTasks", commonDef, 0f, 4f, 1f, "", parent);
        longTasksOption = Create(id + 20001, type, Color, "NumLongTasks", longDef, 0f, 15f, 1f, "", parent);
        shortTasksOption = Create(id + 20002, type, Color, "NumShortTasks", shortDef, 0f, 23f, 1f, "", parent);
    }
}

internal class CustomRoleSelectionOption : CustomOption
{
    internal List<RoleId> RoleIds;

    internal RoleId Role
    {
        get
        {
            return RoleIds[selection];
        }
    }

    internal CustomRoleSelectionOption(int id, CustomOptionType type, Color Color, string name, List<RoleId> RoleIds = null, CustomOption parent = null)
    {
        RoleIds ??= Enum.GetValues(typeof(RoleId)).Cast<RoleId>().ToList();

        this.RoleIds = RoleIds;
        var strings = new string[] { OPTION_OFF };

        Init(id, type, Color, name, strings, 0, "", parent, false, false);
    }

    internal override void UpdateSelection(int newSelection)
    {
        if (RoleIds.Count > 0)
        {
            selections = RoleIds.Select(
                x =>
                    x == RoleId.NoRole ? OPTION_OFF :
                    RoleInfoList.AllRoleInfos.First(y => y.RoleId == x).ColorName
                ).ToArray();
        }

        base.UpdateSelection(newSelection);
    }
}

internal class CustomOptionBlank : CustomOption
{
    internal CustomOptionBlank(CustomOption parent)
    {
        this.parent = parent;
        id = -1;
        name = "";
        isHeader = false;
        isHidden = true;
        children = new();
        selections = new string[] { "" };
        options.Add(this);
    }

    internal override int GetSelection()
    {
        return 0;
    }

    internal override bool GetBool()
    {
        return true;
    }

    internal override float GetFloat()
    {
        return 0f;
    }

    internal override string GetValueString()
    {
        return "";
    }

    internal override void UpdateSelection(int newSelection)
    {
        return;
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
class GameOptionsMenuStartPatch
{
    internal static void Postfix(GameOptionsMenu __instance)
    {
        if (GameObject.Find("UMSettings") != null)
        {
            // Settings setup has already been performed, fixing the title of the tab and returning
            GameObject.Find("UMSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TextMeshPro>().SetText(ModResources.TabTitleGeneral);
            return;
        }
        if (GameObject.Find("ImpostorSettings") != null)
        {
            GameObject.Find("ImpostorSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TextMeshPro>().SetText(ModResources.TabTitleImp);
            return;
        }
        if (GameObject.Find("NeutralSettings") != null)
        {
            GameObject.Find("NeutralSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TextMeshPro>().SetText(ModResources.TabTitleNeu);
            return;
        }
        if (GameObject.Find("CrewmateSettings") != null)
        {
            GameObject.Find("CrewmateSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TextMeshPro>().SetText(ModResources.TabTitleCrew);
            return;
        }
        if (GameObject.Find("ModifierSettings") != null)
        {
            GameObject.Find("ModifierSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TextMeshPro>().SetText(ModResources.TabTitleMod);
            return;
        }
        if (GameObject.Find("OtherSettings") != null)
        {
            GameObject.Find("OtherSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TextMeshPro>().SetText(ModResources.TabTitleOth);
            return;
        }

        // Setup UM tab
        var template = Object.FindObjectsOfType<StringOption>().FirstOrDefault();
        if (template == null) return;
        var gameSettings = GameObject.Find("Game Settings");
        var gameSettingMenu = Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        var umSettings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
        var umMenu = umSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        umSettings.name = "UMSettings";

        var impostorSettings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
        var impostorMenu = impostorSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        impostorSettings.name = "ImpostorSettings";

        var neutralSettings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
        var neutralMenu = neutralSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        neutralSettings.name = "NeutralSettings";

        var crewmateSettings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
        var crewmateMenu = crewmateSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        crewmateSettings.name = "CrewmateSettings";

        var modifierSettings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
        var modifierMenu = modifierSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        modifierSettings.name = "ModifierSettings";

        var otherSettings = Object.Instantiate(gameSettings, gameSettings.transform.parent);
        var otherMenu = otherSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        otherSettings.name = "OtherSettings";

        var roleTab = GameObject.Find("RoleTab");
        var gameTab = GameObject.Find("GameTab");

        var umTab = Object.Instantiate(roleTab, roleTab.transform.parent);
        var umTabHighlight = umTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
        umTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromResources("AmongDogUs.Resources.GeneralTab.png", 100f);

        var impostorTab = Object.Instantiate(roleTab, umTab.transform);
        var impostorTabHighlight = impostorTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
        impostorTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromResources("AmongDogUs.Resources.ImpTab.png", 100f);
        impostorTab.name = "ImpostorTab";

        var neutralTab = Object.Instantiate(roleTab, impostorTab.transform);
        var neutralTabHighlight = neutralTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
        neutralTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromResources("AmongDogUs.Resources.NeutralTab.png", 100f);
        neutralTab.name = "NeutralTab";

        var crewmateTab = Object.Instantiate(roleTab, neutralTab.transform);
        var crewmateTabHighlight = crewmateTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
        crewmateTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromResources("AmongDogUs.Resources.CrewTab.png", 100f);
        crewmateTab.name = "CrewmateTab";

        var modifierTab = Object.Instantiate(roleTab, crewmateTab.transform);
        var modifierTabHighlight = modifierTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
        modifierTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromResources("AmongDogUs.Resources.ModifierTab.png", 100f);
        modifierTab.name = "ModifierTab";

        var otherTab = Object.Instantiate(roleTab, modifierTab.transform);
        var otherTabHighlight = otherTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
        otherTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromResources("AmongDogUs.Resources.OtherTab.png", 100f);
        otherTab.name = "OtherTab";

        // Position of Tab Icons
        gameTab.transform.position += Vector3.left * 3.2f;
        roleTab.transform.position += Vector3.left * 3.2f;
        umTab.transform.position += Vector3.left * 2.1f;
        impostorTab.transform.localPosition = Vector3.right * 0.9f;
        neutralTab.transform.localPosition = Vector3.right * 0.9f;
        crewmateTab.transform.localPosition = Vector3.right * 0.9f;
        modifierTab.transform.localPosition = Vector3.right * 0.9f;
        otherTab.transform.localPosition = Vector3.right * 0.9f;

        var tabs = new GameObject[] { gameTab, roleTab, umTab, impostorTab, neutralTab, crewmateTab, modifierTab, otherTab };
        for (int i = 0; i < tabs.Length; i++)
        {
            var button = tabs[i].GetComponentInChildren<PassiveButton>();
            if (button == null) continue;
            int copiedIndex = i;
            button.OnClick = new ButtonClickedEvent();
            button.OnClick.AddListener((UnityAction)(() =>
            {
                gameSettingMenu.RegularGameSettings.SetActive(false);
                gameSettingMenu.RolesSettings.gameObject.SetActive(false);
                umSettings.gameObject.SetActive(false);
                impostorSettings.gameObject.SetActive(false);
                neutralSettings.gameObject.SetActive(false);
                crewmateSettings.gameObject.SetActive(false);
                modifierSettings.gameObject.SetActive(false);
                otherSettings.gameObject.SetActive(false);
                gameSettingMenu.GameSettingsHightlight.enabled = false;
                gameSettingMenu.RolesSettingsHightlight.enabled = false;
                umTabHighlight.enabled = false;
                impostorTabHighlight.enabled = false;
                neutralTabHighlight.enabled = false;
                crewmateTabHighlight.enabled = false;
                modifierTabHighlight.enabled = false;
                otherTabHighlight.enabled = false;
                if (copiedIndex == 0)
                {
                    gameSettingMenu.RegularGameSettings.SetActive(true);
                    gameSettingMenu.GameSettingsHightlight.enabled = true;
                }
                else if (copiedIndex == 1)
                {
                    gameSettingMenu.RolesSettings.gameObject.SetActive(true);
                    gameSettingMenu.RolesSettingsHightlight.enabled = true;
                }
                else if (copiedIndex == 2)
                {
                    umSettings.gameObject.SetActive(true);
                    umTabHighlight.enabled = true;
                }
                else if (copiedIndex == 3)
                {
                    impostorSettings.gameObject.SetActive(true);
                    impostorTabHighlight.enabled = true;
                }
                else if (copiedIndex == 4)
                {
                    neutralSettings.gameObject.SetActive(true);
                    neutralTabHighlight.enabled = true;
                }
                else if (copiedIndex == 5)
                {
                    crewmateSettings.gameObject.SetActive(true);
                    crewmateTabHighlight.enabled = true;
                }
                else if (copiedIndex == 6)
                {
                    modifierSettings.gameObject.SetActive(true);
                    modifierTabHighlight.enabled = true;
                }
                else if (copiedIndex == 7)
                {
                    otherSettings.gameObject.SetActive(true);
                    otherTabHighlight.enabled = true;
                }
            }));
        }

        foreach (OptionBehaviour option in umMenu.GetComponentsInChildren<OptionBehaviour>()) Object.Destroy(option.gameObject);
        List<OptionBehaviour> umOptions = new();

        foreach (OptionBehaviour option in impostorMenu.GetComponentsInChildren<OptionBehaviour>()) Object.Destroy(option.gameObject);
        List<OptionBehaviour> impostorOptions = new();

        foreach (OptionBehaviour option in neutralMenu.GetComponentsInChildren<OptionBehaviour>()) Object.Destroy(option.gameObject);
        List<OptionBehaviour> neutralOptions = new();

        foreach (OptionBehaviour option in crewmateMenu.GetComponentsInChildren<OptionBehaviour>()) Object.Destroy(option.gameObject);
        List<OptionBehaviour> crewmateOptions = new();

        foreach (OptionBehaviour option in modifierMenu.GetComponentsInChildren<OptionBehaviour>()) Object.Destroy(option.gameObject);
        List<OptionBehaviour> modifierOptions = new();

        foreach (OptionBehaviour option in otherMenu.GetComponentsInChildren<OptionBehaviour>()) Object.Destroy(option.gameObject);
        List<OptionBehaviour> otherOptions = new();

        List<Transform> menus = new() { umMenu.transform, impostorMenu.transform, neutralMenu.transform, crewmateMenu.transform, modifierMenu.transform, otherMenu.transform };
        List<List<OptionBehaviour>> optionBehaviours = new() { umOptions, impostorOptions, neutralOptions, crewmateOptions, modifierOptions, otherOptions };

        for (int i = 0; i < CustomOption.options.Count; i++)
        {
            CustomOption option = CustomOption.options[i];
            if (option.optionBehaviour == null)
            {
                StringOption stringOption = Object.Instantiate(template, menus[(int)option.type]);
                optionBehaviours[(int)option.type].Add(stringOption);
                stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                stringOption.TitleText.text = option.name;
                stringOption.Value = stringOption.oldValue = option.selection;
                stringOption.ValueText.text = option.selections[option.selection].ToString();

                option.optionBehaviour = stringOption;
            }
            option.optionBehaviour.gameObject.SetActive(true);
        }

        umMenu.Children = umOptions.ToArray();
        umSettings.gameObject.SetActive(false);

        impostorMenu.Children = impostorOptions.ToArray();
        impostorSettings.gameObject.SetActive(false);

        neutralMenu.Children = neutralOptions.ToArray();
        neutralSettings.gameObject.SetActive(false);

        crewmateMenu.Children = crewmateOptions.ToArray();
        crewmateSettings.gameObject.SetActive(false);

        modifierMenu.Children = modifierOptions.ToArray();
        modifierSettings.gameObject.SetActive(false);

        otherMenu.Children = otherOptions.ToArray();
        otherSettings.gameObject.SetActive(false);

        /*var NumImpostorsOption = __instance.Children.FirstOrDefault(x => x.name == "NumImpostors").TryCast<NumberOption>();
        if (NumImpostorsOption != null) NumImpostorsOption.ValidRange = new FloatRange(0f, 15f);*/

        var killCoolOption = __instance.Children.FirstOrDefault(x => x.name == "KillCooldown").TryCast<NumberOption>();
        if (killCoolOption != null) killCoolOption.ValidRange = new FloatRange(2.5f, 60f);

        var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
        if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

        var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
        if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

        var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
        if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
    }
}

[HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
internal class KeyValueOptionEnablePatch
{
    internal static void Postfix(KeyValueOption __instance)
    {
        if (__instance.Title == StringNames.GameMapName)
        {
            __instance.Selected = GameManager.Instance.LogicOptions.currentGameOptions.GetByte(ByteOptionNames.MapId);
        }
        try
        {
            __instance.ValueText.text = __instance.Values[Mathf.Clamp(__instance.Selected, 0, __instance.Values.Count - 1)].Key;
        }
        catch { }
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
internal class StringOptionEnablePatch
{
    internal static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);

        if (option == null) return true;

        SpriteRenderer Background = option.optionBehaviour.transform.Find("Background").GetComponent<SpriteRenderer>();
        __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
        __instance.TitleText.text = option.name;
        __instance.Value = __instance.oldValue = option.selection;
        __instance.ValueText.text = option.GetValueString();
        Background.color = option.Color;
        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
internal class StringOptionIncreasePatch
{
    internal static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
        if (option == null) return true;
        option.UpdateSelection(option.selection + 1);
        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
internal class StringOptionDecreasePatch
{
    internal static bool Prefix(StringOption __instance)
    {
        CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
        if (option == null) return true;
        option.UpdateSelection(option.selection - 1);
        return false;
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
internal class AmongUsClientOnPlayerJoinedPatch
{
    internal static void Postfix()
    {
        CustomOption.ShareOptionSelections();
    }
}


[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
class GameOptionsMenuUpdatePatch
{
    private static float timer = 1f;
    internal static void Postfix(GameOptionsMenu __instance)
    {
        foreach (var ob in __instance.Children)
        {
            switch (ob.Title)
            {
                case StringNames.GameRecommendedSettings:
                    ob.enabled = false;
                    ob.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        // Return Menu Update if in normal among us settings
        var gameSettingMenu = Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu.RegularGameSettings.active || gameSettingMenu.RolesSettings.gameObject.active) return;

        timer += Time.deltaTime;
        if (timer < 0.1f) return;
        timer = 0f;

        float NumItems = __instance.Children.Length;

        float offset = 2.75f;
        foreach (CustomOption option in CustomOption.options)
        {
            if (GameObject.Find("UMSettings") && option.type != CustomOptionType.General) continue;
            if (GameObject.Find("ImpostorSettings") && option.type != CustomOptionType.Impostor) continue;
            if (GameObject.Find("NeutralSettings") && option.type != CustomOptionType.Neutral) continue;
            if (GameObject.Find("CrewmateSettings") && option.type != CustomOptionType.Crewmate) continue;
            if (GameObject.Find("ModifierSettings") && option.type != CustomOptionType.Modifiers) continue;
            if (GameObject.Find("OtherSettings") && option.type != CustomOptionType.Other) continue;
            if (option?.optionBehaviour != null && option.optionBehaviour.gameObject != null)
            {
                bool enabled = true;
                var parent = option.parent;

                if (option.isHidden)
                {
                    enabled = false;
                }

                while (parent != null && enabled)
                {
                    enabled = parent.Enabled;
                    parent = parent.parent;
                }

                option.optionBehaviour.gameObject.SetActive(enabled);
                if (enabled)
                {
                    offset -= option.isHeader ? 0.75f : 0.5f;
                    option.optionBehaviour.transform.localPosition = new Vector3(option.optionBehaviour.transform.localPosition.x, offset, option.optionBehaviour.transform.localPosition.z);

                    if (option.isHeader)
                    {
                        NumItems += 0.5f;
                    }
                }
                else
                {
                    NumItems--;
                }
            }
        }
        __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -4.0f + NumItems * 0.5f;
    }
}

[HarmonyPatch(nameof(HudManager), nameof(HudManager.Update))]
internal static class GameOptionsDataPatch
{
    internal static int NumPages;
    internal static string OptionToString(CustomOption option)
    {
        if (option == null) return "";
        return $"{option.name}: {option.GetValueString()}";
    }

    internal static string OptionsToString(CustomOption option, bool skipFirst = false)
    {
        if (option == null)
        {
            Main.Logger.LogWarning("no option?");
            return "";
        }

        List<string> options = new();
        if (!option.isHidden && !skipFirst) options.Add(OptionToString(option));
        if (option.Enabled)
        {
            foreach (CustomOption op in option.children)
            {
                string str = OptionsToString(op);
                if (str != "") options.Add(str);
            }
        }
        return string.Join("\n", options);
    }

    private static void Postfix()
    {
        List<string> pages = new()
        {
            GameOptionsManager.Instance.CurrentGameOptions.ToHudString(PlayerControl.AllPlayerControls.Count)
        };

        StringBuilder entry = new();
        List<string> entries = new()
        {
            // First add the Presets and the role counts
            OptionToString(CustomOptionsHolder.PresetSelection),
            OptionToString(CustomOptionsHolder.ActivateModRoles),
            OptionToString(CustomOptionsHolder.EnableMirrorMap),
            OptionToString(CustomOptionsHolder.CanZoomInOutWhenPlayerIsDead),
            OptionToString(CustomOptionsHolder.CrewmateRolesCountMin),
            OptionToString(CustomOptionsHolder.CrewmateRolesCountMax),
            OptionToString(CustomOptionsHolder.ImpostorRolesCountMin),
            OptionToString(CustomOptionsHolder.ImpostorRolesCountMax),
            OptionToString(CustomOptionsHolder.NeutralRolesCountMin),
            OptionToString(CustomOptionsHolder.NeutralRolesCountMax),
            OptionToString(CustomOptionsHolder.ModifierCountMin),
            OptionToString(CustomOptionsHolder.ModifierCountMax),
        };

        static void addChildren(CustomOption option, StringBuilder entry, bool indent = true)
        {
            if (!option.Enabled) return;

            foreach (var child in option.children)
            {
                if (!child.isHidden) entry.AppendLine((indent ? "    " : "") + OptionToString(child));
                addChildren(child, entry, indent);
            }
        }

        foreach (CustomOption option in CustomOption.options)
        {
            if ((option == CustomOptionsHolder.PresetSelection) ||
                (option == CustomOptionsHolder.ActivateModRoles) ||
                (option == CustomOptionsHolder.EnableMirrorMap) ||
                (option == CustomOptionsHolder.CanZoomInOutWhenPlayerIsDead) ||
                (option == CustomOptionsHolder.CrewmateRolesCountMin) ||
                (option == CustomOptionsHolder.CrewmateRolesCountMax) ||
                (option == CustomOptionsHolder.ImpostorRolesCountMin) ||
                (option == CustomOptionsHolder.ImpostorRolesCountMax) ||
                (option == CustomOptionsHolder.NeutralRolesCountMin) ||
                (option == CustomOptionsHolder.NeutralRolesCountMax) ||
                (option == CustomOptionsHolder.ModifierCountMin) ||
                (option == CustomOptionsHolder.ModifierCountMax)) continue;

            if (option.parent == null)
            {
                if (!option.Enabled) continue;
                if (!option.isHidden) entry.AppendLine(OptionToString(option));

                addChildren(option, entry, !option.isHidden);
                entries.Add(entry.ToString().Trim('\r', '\n'));
            }
        }

        int MaxLines = 28;
        int lineCount = 0;
        string page = "";

        foreach (var e in entries)
        {
            int lines = e.Count(c => c == '\n') + 1;
            if (lineCount + lines > MaxLines)
            {
                pages.Add(page);
                page = "";
                lineCount = 0;
            }
            page = page + e + "\n";
            lineCount += lines + 1;
        }

        page = page.Trim('\r', '\n');
        if (page != "")
        {
            pages.Add(page);
        }

        NumPages = pages.Count;
        int counter = Main.OptionsPage %= NumPages;

        FastDestroyableSingleton<HudManager>.Instance.GameSettings.text = new StringBuilder(pages[counter].Trim('\r', '\n')).Append("\n\n").Append(ModResources.ChangePage).Append($" ({counter + 1}/{NumPages})").ToString();
    }
}

[HarmonyPatch(typeof(KeyboardJoystick))]
internal static class GameOptionsNextPagePatch
{
    [HarmonyPatch(nameof(KeyboardJoystick.Update)), HarmonyPostfix]
    internal static void UpdateKeyBoard()
    {
        int page = Main.OptionsPage;
        if (Input.GetKeyDown(KeyCode.Period))
        {
            Main.OptionsPage = (Main.OptionsPage + 1) % 7;
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            if (Main.OptionsPage > 0) Main.OptionsPage -= 1;
            else if (Main.OptionsPage == 0) Main.OptionsPage = GameOptionsDataPatch.NumPages - 1;
        }
        if (page != Main.OptionsPage)
        {
            Vector3 position = (Vector3)FastDestroyableSingleton<HudManager>.Instance?.GameSettings?.transform.localPosition;
            FastDestroyableSingleton<HudManager>.Instance.GameSettings.transform.localPosition = new Vector3(position.x, 2.9f, position.z);
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
internal class GameSettingsScalePatch
{
    internal static void Prefix(HudManager __instance)
    {
        if (__instance.GameSettings != null) __instance.GameSettings.fontSize = 1.2f;
    }
}

[HarmonyPatch(typeof(GameOptionsData))]
internal static class GameOptionsDeserializePatch
{
    static private int NumImpostors = GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors;

    [HarmonyPatch(nameof(GameOptionsData.Deserialize)), HarmonyPrefix]
    internal static bool Prefix()
    {
        NumImpostors = GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors;
        return true;
    }

    [HarmonyPatch(nameof(GameOptionsData.Deserialize)), HarmonyPostfix]
    internal static void Postfix()
    {
        try
        {
            GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.NumImpostors, NumImpostors);
        }
        catch { }
    }
}

internal static class CustomOptionsHolder
{
    internal static string[] TenRates = new[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
    internal static string[] HundredRates = new[] { "0%", "100%" };
    internal static string[] Presets = new[] { ModResources.Preset1, ModResources.Preset2, ModResources.Preset3, ModResources.Preset4, ModResources.Preset5 };

    internal static CustomOption PresetSelection;
    internal static CustomOption ActivateModRoles;
    internal static CustomOption CrewmateRolesCountMin;
    internal static CustomOption CrewmateRolesCountMax;
    internal static CustomOption ImpostorRolesCountMin;
    internal static CustomOption ImpostorRolesCountMax;
    internal static CustomOption NeutralRolesCountMin;
    internal static CustomOption NeutralRolesCountMax;
    internal static CustomOption ModifierCountMin;
    internal static CustomOption ModifierCountMax;
    // public static CustomOption RememberClassic;

    internal static CustomOption SpecialOptions;
    internal static CustomOption MaxNumberOfMeetings;
    internal static CustomOption BlockSkippingInEmergencyMeetings;
    internal static CustomOption NoVoteIsSelfVote;
    internal static CustomOption AllowParallelMedBayScans;
    internal static CustomOption HideOutOfSightNameTags;
    internal static CustomOption HidePlayerNames;
    internal static CustomOption RefundVotesOnDeath;
    internal static CustomOption EnableMirrorMap;
    internal static CustomOption CanZoomInOutWhenPlayerIsDead;
    internal static CustomOption FillCrewmate;

    internal static CustomOption RandomMap;
    internal static CustomOption RandomMapEnableSkeld;
    internal static CustomOption RandomMapEnableMira;
    internal static CustomOption RandomMapEnablePolus;
    internal static CustomOption RandomMapEnableAirShip;
    internal static CustomOption RandomMapEnableSubmerged;

    internal static CustomOption RestrictDevices;
    internal static CustomOption RestrictAdmin;
    internal static CustomOption RestrictCameras;
    internal static CustomOption RestrictVitals;

    // public static CustomOption EnableGodMiraHQ;

    internal static CustomOption AirShipSettings;
    internal static CustomOption OldAirShipAdmin;
    internal static CustomOption AirshipReactorDuration;
    internal static CustomOption EnableRecordsAdmin;
    internal static CustomOption EnableCockpitAdmin;

    internal static Dictionary<byte, byte[]> BlockedRolePairings = new();

    internal static string ColorString(Color c, string s)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    internal static void Load()
    {
        // Role Options
        ActivateModRoles = CustomOption.Create(1, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.ActivateRoles), true, "", null, true);
        PresetSelection = CustomOption.Create(3, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.PresetSelection), Presets, "", null, true);

        // RememberClassic = Create(4, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "RememberClassic"), false, null, true);
        EnableMirrorMap = CustomOption.Create(5, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.MirrorMap), false, "", null, true);
        CanZoomInOutWhenPlayerIsDead = CustomOption.Create(6, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.CanZoomInOutDead), true, "", null);
        FillCrewmate = CustomOption.Create(7, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.FillCrewmate), false, "", null);
        // EnableGodMiraHQ = Create(7, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "EnableGodMiraHQ"), false, null, true);

        // Using new id's for the options to not break compatibility with older versions
        CrewmateRolesCountMin = CustomOption.Create(10, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.CrewmateRolesCountMin), 0f, 0f, 15f, 1f, ModResources.FormatPlayer, null, true);
        CrewmateRolesCountMax = CustomOption.Create(11, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.CrewmateRolesCountMax), 0f, 0f, 15f, 1f, ModResources.FormatPlayer, null, true);
        ImpostorRolesCountMin = CustomOption.Create(12, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.ImpostorRolesCountMin), 0f, 0f, 15f, 1f, ModResources.FormatPlayer);
        ImpostorRolesCountMax = CustomOption.Create(13, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.ImpostorRolesCountMax), 0f, 0f, 15f, 1f, ModResources.FormatPlayer);
        NeutralRolesCountMin = CustomOption.Create(14, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.NeutralRolesCountMin), 0f, 0f, 15f, 1f, ModResources.FormatPlayer);
        NeutralRolesCountMax = CustomOption.Create(15, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.NeutralRolesCountMax), 0f, 0f, 15f, 1f, ModResources.FormatPlayer);
        ModifierCountMin = CustomOption.Create(16, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.ModifierCountMin), 0f, 0f, 15f, 1f, ModResources.FormatPlayer);
        ModifierCountMax = CustomOption.Create(17, CustomOptionType.General, Color.yellow, ColorString(new Color(204f / 255f, 204f / 255f, 0, 1f), ModResources.ModifierCountMax), 0f, 0f, 15f, 1f, ModResources.FormatPlayer);

        SpecialOptions = new CustomOptionBlank(null);
        MaxNumberOfMeetings = CustomOption.Create(20, CustomOptionType.General, Color.white, ModResources.MaxNumberOfMeetings, 10f, 0f, 15f, 1f, ModResources.FormatTimes, SpecialOptions, true);
        BlockSkippingInEmergencyMeetings = CustomOption.Create(21, CustomOptionType.General, Color.white, ModResources.BlockSkip, false, "", SpecialOptions);
        NoVoteIsSelfVote = CustomOption.Create(22, CustomOptionType.General, Color.white, ModResources.NoVoteIsSelfVote, false, "", SpecialOptions);
        AllowParallelMedBayScans = CustomOption.Create(23, CustomOptionType.General, Color.white, ModResources.NoMedBayLimit, true, "", SpecialOptions);
        HideOutOfSightNameTags = CustomOption.Create(24, CustomOptionType.General, Color.white, ModResources.HideOutName, true, "", SpecialOptions);
        HidePlayerNames = CustomOption.Create(25, CustomOptionType.General, Color.white, ModResources.HidePlayerName, false, "", SpecialOptions);
        RefundVotesOnDeath = CustomOption.Create(26, CustomOptionType.General, Color.white, ModResources.RefundVoteDeath, true, "", SpecialOptions);
        // EnableGodMiraHQ = Create(27, General, "EnableGodMira", false, SpecialOptions);
        RandomMap = CustomOption.Create(34, CustomOptionType.General, Color.white, ModResources.PlayRandomMaps, false, "", SpecialOptions);
        RandomMapEnableSkeld = CustomOption.Create(50, CustomOptionType.General, Color.white, ModResources.RandomMapsEnableSkeld, true, "", RandomMap, false);
        RandomMapEnableMira = CustomOption.Create(51, CustomOptionType.General, Color.white, ModResources.RandomMapsEnableMira, true, "", RandomMap, false);
        RandomMapEnablePolus = CustomOption.Create(52, CustomOptionType.General, Color.white, ModResources.RandomMapsEnablePolus, true, "", RandomMap, false);
        RandomMapEnableAirShip = CustomOption.Create(53, CustomOptionType.General, Color.white, ModResources.RandomMapsEnableAirShip, true, "", RandomMap, false);
        RandomMapEnableSubmerged = CustomOption.Create(54, CustomOptionType.General, Color.white, ModResources.RandomMapsEnableSubmerged, true, "", RandomMap, false);
        RestrictDevices = CustomOption.Create(60, CustomOptionType.General, Color.white, ModResources.RestrictDevices, new string[] { CustomOption.OPTION_OFF, ModResources.RestrictPerTurn, ModResources.RestrictPerGame }, "", SpecialOptions);
        RestrictAdmin = CustomOption.Create(61, CustomOptionType.General, Color.white, ModResources.DisableAdmin, 30f, 0f, 600f, 5f, ModResources.FormatSeconds, RestrictDevices);
        RestrictCameras = CustomOption.Create(62, CustomOptionType.General, Color.white, ModResources.DisableCameras, 30f, 0f, 600f, 5f, ModResources.FormatSeconds, RestrictDevices);
        RestrictVitals = CustomOption.Create(63, CustomOptionType.General, Color.white, ModResources.DisableVitals, 30f, 0f, 600f, 5f, ModResources.FormatSeconds, RestrictDevices);

        AirShipSettings = CustomOption.Create(80, CustomOptionType.General, Color.white, ModResources.AirShipSettings, false, "", SpecialOptions);
        OldAirShipAdmin = CustomOption.Create(81, CustomOptionType.General, Color.white, ModResources.OldAirShipAdmin, true, "", AirShipSettings);
        EnableRecordsAdmin = CustomOption.Create(82, CustomOptionType.General, Color.white, ModResources.EnableRecordsAdmin, false, "", AirShipSettings);
        EnableCockpitAdmin = CustomOption.Create(83, CustomOptionType.General, Color.white, ModResources.EnableCockpitAdmin, false, "", AirShipSettings);
        AirshipReactorDuration = CustomOption.Create(84, CustomOptionType.General, Color.white, ModResources.AirShipReactorDuration, 90f, 10f, 600f, 5f, ModResources.FormatSeconds, AirShipSettings);
    }
}