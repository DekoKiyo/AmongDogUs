namespace AmongDogUs.Modules;

[HarmonyPatch]
internal static class ClientOptionsPatch
{
    private static readonly SelectionBehaviour[] AllOptions = {
        new SelectionBehaviour(ModResources.GhostsSeeTasksButton, () => ModMapOptions.GhostsSeeTasks = Main.GhostsSeeTasks.Value = !Main.GhostsSeeTasks.Value, Main.GhostsSeeTasks.Value),
        new SelectionBehaviour(ModResources.GhostsSeeVotesButton, () => ModMapOptions.GhostsSeeVotes = Main.GhostsSeeVotes.Value = !Main.GhostsSeeVotes.Value, Main.GhostsSeeVotes.Value),
        new SelectionBehaviour(ModResources.GhostsSeeRolesButton, () => ModMapOptions.GhostsSeeRoles = Main.GhostsSeeRoles.Value = !Main.GhostsSeeRoles.Value, Main.GhostsSeeRoles.Value),
        new SelectionBehaviour(ModResources.ShowRoleSummaryButton, () => ModMapOptions.ShowRoleSummary = Main.ShowRoleSummary.Value = !Main.ShowRoleSummary.Value, Main.ShowRoleSummary.Value),
    };

    private static GameObject popUp;
    private static TextMeshPro titleText;

    private static ToggleButtonBehaviour buttonPrefab;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    internal static void MainMenuManager_StartPostfix(MainMenuManager __instance)
    {
        var go = new GameObject("TitleTextVan");
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.transform.localPosition += Vector3.left * 0.2f;
        titleText = Object.Instantiate(tmp);
        titleText.gameObject.SetActive(false);
        Object.DontDestroyOnLoad(titleText);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    internal static void OptionsMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
    {
        if (!__instance.CensorChatButton) return;

        if (!popUp) CreateCustom(__instance);

        if (!buttonPrefab)
        {
            buttonPrefab = Object.Instantiate(__instance.CensorChatButton);
            Object.DontDestroyOnLoad(buttonPrefab);
            buttonPrefab.name = "CensorChatPrefab";
            buttonPrefab.gameObject.SetActive(false);
        }

        SetUpOptions();
        InitializeMoreButton(__instance);
    }

    private static void CreateCustom(OptionsMenuBehaviour prefab)
    {
        popUp = Object.Instantiate(prefab.gameObject);
        Object.DontDestroyOnLoad(popUp);
        var transform = popUp.transform;
        var pos = transform.localPosition;
        pos.z = -810f;
        transform.localPosition = pos;

        Object.Destroy(popUp.GetComponent<OptionsMenuBehaviour>());
        foreach (var gObj in popUp.gameObject.GetAllChilds())
        {
            if (gObj.name != "Background" && gObj.name != "CloseButton") Object.Destroy(gObj);
        }
        popUp.SetActive(false);
    }

    private static void InitializeMoreButton(OptionsMenuBehaviour __instance)
    {
        var moreOptions = Object.Instantiate(buttonPrefab, __instance.CensorChatButton.transform.parent);
        moreOptions.transform.localPosition = __instance.CensorChatButton.transform.localPosition + Vector3.right * 2.6f;

        moreOptions.gameObject.SetActive(true);
        moreOptions.Text.text = ModResources.ModOptionsText;
        var moreOptionsButton = moreOptions.GetComponent<PassiveButton>();
        moreOptionsButton.OnClick = new ButtonClickedEvent();
        moreOptionsButton.OnClick.AddListener((Action)(() =>
        {
            if (!popUp) return;
            if (__instance.transform.parent && __instance.transform.parent == FastDestroyableSingleton<HudManager>.Instance.transform)
            {
                popUp.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                popUp.transform.localPosition = new Vector3(0, 0, -800f);
            }
            else
            {
                popUp.transform.SetParent(null);
                Object.DontDestroyOnLoad(popUp);
            }
            CheckSetTitle();
            RefreshOpen();
        }));
    }

    private static void RefreshOpen()
    {
        popUp.gameObject.SetActive(false);
        popUp.gameObject.SetActive(true);
        SetUpOptions();
    }

    private static void CheckSetTitle()
    {
        if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !titleText) return;

        var title = Object.Instantiate(titleText, popUp.transform);
        title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
        title.gameObject.SetActive(true);
        title.text = ModResources.ModOptionsText;
        title.name = "TitleText";
    }

    private static void SetUpOptions()
    {
        if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

        for (var i = 0; i < AllOptions.Length; i++)
        {
            var info = AllOptions[i];

            var button = Object.Instantiate(buttonPrefab, popUp.transform);
            var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

            var transform = button.transform;
            transform.localPosition = pos;

            button.onState = info.DefaultValue;
            button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

            button.Text.text = info.Title;
            button.Text.fontSizeMin = button.Text.fontSizeMax = 2f;
            button.Text.font = Object.Instantiate(titleText.font);
            button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

            button.name = info.Title.Replace(" ", "") + "Toggle";
            button.gameObject.SetActive(true);

            var passiveButton = button.GetComponent<PassiveButton>();
            var colliderButton = button.GetComponent<BoxCollider2D>();

            colliderButton.size = new Vector2(2.2f, .7f);

            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnMouseOut = new UnityEvent();
            passiveButton.OnMouseOver = new UnityEvent();

            passiveButton.OnClick.AddListener((Action)(() =>
            {
                button.onState = info.OnClick();
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
            }));

            passiveButton.OnMouseOver.AddListener((Action)(() => button.Background.color = new Color32(34, 139, 34, byte.MaxValue)));
            passiveButton.OnMouseOut.AddListener((Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

            foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>()) spr.size = new Vector2(2.2f, .7f);
        }
    }

    private static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
    {
        for (var i = 0; i < Go.transform.childCount; i++)
        {
            yield return Go.transform.GetChild(i).gameObject;
        }
    }

    internal class SelectionBehaviour
    {
        internal string Title;
        internal Func<bool> OnClick;
        internal bool DefaultValue;

        internal SelectionBehaviour(string title, Func<bool> onClick, bool defaultValue)
        {
            Title = title;
            OnClick = onClick;
            DefaultValue = defaultValue;
        }
    }
}