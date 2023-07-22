// Source Code from TheOtherRoles(https://github.com/Eisbison/TheOtherRoles)

using Action = System.Action;
using Version = SemanticVersioning.Version;

namespace AmongDogUs.Modules;

internal class ModUpdateBehaviour : MonoBehaviour
{
    internal static readonly bool CheckForSubmergedUpdates = true;
    internal static bool ShowPopUp = true;
    internal static bool UpdateInProgress = false;

    internal static ModUpdateBehaviour Instance { get; private set; }
    internal ModUpdateBehaviour(IntPtr ptr) : base(ptr) { }
    internal class UpdateData
    {
        internal string Content;
        internal string Tag;
        internal string TimeString;
        internal JObject Request;
        internal Version Version => Version.Parse(Tag);

        internal UpdateData(JObject data)
        {
            Tag = data["tag_name"]?.ToString().TrimStart('v');
            Content = data["body"]?.ToString();
            TimeString = DateTime.FromBinary(((Il2CppSystem.DateTime)data["published_at"]).ToBinaryRaw()).ToString();
            Request = data;
        }

        internal bool IsNewer(Version version)
        {
            if (!Version.TryParse(Tag, out var myVersion)) return false;
            return myVersion.BaseVersion() > version.BaseVersion();
        }
    }

    internal UpdateData ADUUpdate;
    internal UpdateData SubmergedUpdate;

    [HideFromIl2Cpp]
    internal UpdateData RequiredUpdateData => ADUUpdate ?? SubmergedUpdate;

    internal void Awake()
    {
        if (Instance) Destroy(this);
        Instance = this;

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)OnSceneLoaded);
        this.StartCoroutine(CoCheckUpdates());

        foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.old")) File.Delete(file);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (UpdateInProgress || scene.name != "MainMenu") return;
        if (RequiredUpdateData is null)
        {
            ShowPopUp = false;
            return;
        }

        var template = GameObject.Find("ExitGameButton");
        if (!template) return;

        var button = Instantiate(template, null);
        var buttonTransform = button.transform;
        // buttonTransform.localPosition = new Vector3(-2f, -2f);
        button.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.458f, 0.124f);

        PassiveButton passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((Action)(() =>
        {
            this.StartCoroutine(CoUpdate());
            button.SetActive(false);
        }));

        var text = button.transform.GetComponentInChildren<TMP_Text>();
        string t = ModResources.UpdateADU;
        if (ADUUpdate is null && SubmergedUpdate is not null) t = SubmergedCompatibility.Loaded ? ModResources.UpdateSubmerged : ModResources.DownloadSubmerged;

        StartCoroutine(Effects.Lerp(0.1f, (Action<float>)(p => text.SetText(t))));
        passiveButton.OnMouseOut.AddListener((Action)(() => text.color = Color.red));
        passiveButton.OnMouseOver.AddListener((Action)(() => text.color = Color.white));

        var isSubmerged = ADUUpdate == null;
        var announcement = string.Format(ModResources.AnnouncementUpdate, isSubmerged ? "Submerged" : "AmongDogUs", isSubmerged ? SubmergedUpdate.Tag : ADUUpdate.Tag, isSubmerged ? SubmergedUpdate.Content : ADUUpdate.Content);
        var mgr = FindObjectOfType<MainMenuManager>(true);

        if (!isSubmerged)
        {
            try
            {
                string updateVersion = ADUUpdate.Content[^5..];
                if (Version.Parse(Main.PLUGIN_VERSION).BaseVersion() < Version.Parse(updateVersion).BaseVersion())
                {
                    passiveButton.OnClick.RemoveAllListeners();
                    passiveButton.OnClick = new();
                    passiveButton.OnClick.AddListener((Action)(() =>
                    {
                        mgr.StartCoroutine(CoShowAnnouncement(ModResources.ManualUpdate));
                    }));
                }
            }
            catch
            {
                Main.Logger.LogError("parsing version for auto updater failed :(");
            }

        }

        if (isSubmerged && !SubmergedCompatibility.Loaded) ShowPopUp = false;
        if (ShowPopUp) mgr.StartCoroutine(CoShowAnnouncement(announcement, shortTitle: isSubmerged ? ModResources.SubmergedUpdateTitle : ModResources.ADUUpdateTitle, date: isSubmerged ? SubmergedUpdate.TimeString : ADUUpdate.TimeString));
        ShowPopUp = false;
    }

    [HideFromIl2Cpp]
    internal IEnumerator CoUpdate()
    {
        UpdateInProgress = true;
        var isSubmerged = ADUUpdate is null;
        var updateName = isSubmerged ? "Submerged" : "AmongDogUs";

        var popup = Instantiate(TwitchManager.Instance.TwitchPopup);
        popup.TextAreaTMP.fontSize *= 0.7f;
        popup.TextAreaTMP.enableAutoSizing = false;

        popup.Show();

        var button = popup.transform.GetChild(2).gameObject;
        button.SetActive(false);
        popup.TextAreaTMP.text = string.Format(ModResources.UpdatingProcessText, updateName);

        var download = Task.Run(DownloadUpdate);
        while (!download.IsCompleted) yield return null;

        button.SetActive(true);
        popup.TextAreaTMP.text = download.Result ? string.Format(ModResources.UpdateSucceed, updateName): string.Format(ModResources.UpdateFailed, updateName);
    }

    private static int announcementNumber = 501;

    [HideFromIl2Cpp]
    internal static IEnumerator CoShowAnnouncement(string announcement, bool _ = true, string shortTitle = "ADU Update", string title = "", string date = "")
    {
        var mgr = FindObjectOfType<MainMenuManager>(true);
        var popUpTemplate = FindObjectOfType<AnnouncementPopUp>(true);
        if (popUpTemplate == null)
        {
            Main.Logger.LogError("couldn't show credits, popUp is null");
            yield return null;
        }
        var popUp = Instantiate(popUpTemplate);

        popUp.gameObject.SetActive(true);

        Announcement creditsAnnouncement = new()
        {
            Id = "aduAnnouncement",
            Language = 0,
            Number = announcementNumber++,
            Title = title == "" ? ModResources.ADUUpdateTitle : title,
            ShortTitle = shortTitle,
            SubTitle = "",
            PinState = false,
            Date = date == "" ? DateTime.Now.Date.ToString() : date,
            Text = announcement,
        };
        mgr.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
        {
            if (p == 1)
            {
                var backup = DataManager.Player.Announcements.allAnnouncements;
                DataManager.Player.Announcements.allAnnouncements = new();
                popUp.Init(false);
                DataManager.Player.Announcements.SetAnnouncements(new[] { creditsAnnouncement });
                popUp.CreateAnnouncementList();
                popUp.UpdateAnnouncementText(creditsAnnouncement.Number);
                popUp.visibleAnnouncements[0].PassiveButton.OnClick.RemoveAllListeners();
                DataManager.Player.Announcements.allAnnouncements = backup;
            }
        })));
    }

    [HideFromIl2Cpp]
    internal static IEnumerator CoCheckUpdates()
    {
        var aduUpdateCheck = Task.Run(() => GetGithubUpdate("DekoKiyo", "AmongDogUs"));
        while (!aduUpdateCheck.IsCompleted) yield return null;
        if (aduUpdateCheck.Result != null && aduUpdateCheck.Result.IsNewer(Version.Parse(Main.PLUGIN_VERSION)))
        {
            Instance.ADUUpdate = aduUpdateCheck.Result;
        }
        if (CheckForSubmergedUpdates)
        {
            var submergedUpdateCheck = Task.Run(() => GetGithubUpdate("SubmergedAmongUs", "Submerged"));
            while (!submergedUpdateCheck.IsCompleted) yield return null;
            if (submergedUpdateCheck.Result != null && (!SubmergedCompatibility.Loaded || submergedUpdateCheck.Result.IsNewer(SubmergedCompatibility.Version)))
            {
                Instance.SubmergedUpdate = submergedUpdateCheck.Result;
                if (Instance.SubmergedUpdate.Tag.Equals("v2023.7.17-beta.8")) Instance.SubmergedUpdate = null;
            }
        }
        Instance.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    [HideFromIl2Cpp]
    internal static async Task<UpdateData> GetGithubUpdate(string owner, string repo)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "AmongDogUs Updater");

        try
        {
            var req = await client.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest", HttpCompletionOption.ResponseContentRead);

            if (!req.IsSuccessStatusCode) return null;

            var dataString = await req.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(dataString);
            return new UpdateData(data);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private bool TryUpdateSubmergedInternally()
    {
        if (SubmergedUpdate == null) return false;
        try
        {
            if (!SubmergedCompatibility.LoadedExternally) return false;
            var thisAsm = Assembly.GetCallingAssembly();
            var resourceName = thisAsm.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith("Submerged.dll"));
            if (resourceName == default) return false;

            using var submergedStream = thisAsm.GetManifestResourceStream(resourceName)!;
            var asmDef = AssemblyDefinition.ReadAssembly(submergedStream, TypeLoader.ReaderParameters);
            var pluginType = asmDef.MainModule.Types.FirstOrDefault(t => t.IsSubtypeOf(typeof(BasePlugin)));
            var info = IL2CPPChainloader.ToPluginInfo(pluginType, "");
            if (SubmergedUpdate.IsNewer(info.Metadata.Version)) return false;
            File.Delete(SubmergedCompatibility.Assembly.Location);

        }
        catch (Exception e)
        {
            Main.Logger.LogError(e);
            return false;
        }
        return true;
    }

    [HideFromIl2Cpp]
    internal async Task<bool> DownloadUpdate()
    {
        var isSubmerged = ADUUpdate is null;
        if (isSubmerged && TryUpdateSubmergedInternally()) return true;
        var data = isSubmerged ? SubmergedUpdate : ADUUpdate;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "AmongDogUs Updater");

        JToken assets = data.Request["assets"];
        string downloadURI = "";
        for (JToken current = assets.First; current != null; current = current.Next)
        {
            string browser_download_url = current["browser_download_url"]?.ToString();
            if (browser_download_url != null && current["content_type"] != null)
            {
                if (current["content_type"].ToString().Equals("application/x-msdownload") && browser_download_url.EndsWith(".dll"))
                {
                    downloadURI = browser_download_url;
                    break;
                }
            }
        }

        if (downloadURI.Length == 0) return false;

        var res = await client.GetAsync(downloadURI, HttpCompletionOption.ResponseContentRead);
        string filePath = Path.Combine(Paths.PluginPath, isSubmerged ? "Submerged.dll" : "AmongDogUs.dll");
        if (File.Exists(filePath + ".old")) File.Delete(filePath + ".old");
        if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");

        await using var responseStream = await res.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(filePath);
        await responseStream.CopyToAsync(fileStream);

        return true;
    }
}