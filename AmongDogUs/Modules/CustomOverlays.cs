// Source Code from these mods
// Town of Plus (https://github.com/tugaru1975/TownOfPlus)
// Town of Super (https://github.com/reitou-mugicha/TownOfSuper)

namespace AmongDogUs.Modules;

[HarmonyPatch]
internal static class CustomOverlays
{
    internal static Dictionary<int, PlayerVersion> playerVersions = new();
    private static SpriteRenderer meetingUnderlay;
    private static SpriteRenderer infoUnderlay;
    private static TextMeshPro infoOverlayRules;
    private static TextMeshPro infoOverlayPlayer;
    private static TextMeshPro infoOverlayRoles;

    internal static bool OverlayShown = false;

    internal static bool SendVersion = false;

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    internal class GameStartManagerStartPatch
    {
        internal static void Postfix(GameStartManager __instance)
        {
            SendVersion = false;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    internal class GameStartManagerPatch
    {
        internal static void Postfix(GameStartManager __instance)
        {
            if (PlayerControl.LocalPlayer != null && SendVersion == false)
            {
                SendVersion = true;
            }
        }
    }

    internal static void ResetOverlays()
    {
        HideInfoOverlay();
        Object.Destroy(meetingUnderlay);
        Object.Destroy(infoUnderlay);
        Object.Destroy(infoOverlayRules);
        Object.Destroy(infoOverlayPlayer);
        Object.Destroy(infoOverlayRoles);
        infoOverlayRules = null;
        infoOverlayPlayer = null;
        infoOverlayRoles = null;
        meetingUnderlay = null;
        infoUnderlay = null;
        OverlayShown = false;
    }

    internal static bool Initialize(HudManager __instance)
    {
        if (__instance == null) return false;

        if (meetingUnderlay == null)
        {
            meetingUnderlay = Object.Instantiate(__instance.FullScreen, __instance.transform);
            meetingUnderlay.transform.localPosition = new(0f, 0f, 20f);
            meetingUnderlay.gameObject.SetActive(true);
            meetingUnderlay.enabled = false;
        }

        if (infoUnderlay == null)
        {
            infoUnderlay = Object.Instantiate(meetingUnderlay, __instance.transform);
            infoUnderlay.transform.localPosition = new(0f, 0f, -900f);
            infoUnderlay.gameObject.SetActive(true);
            infoUnderlay.enabled = false;
        }

        if (infoOverlayRules == null)
        {
            infoOverlayRules = Object.Instantiate(__instance.TaskPanel.taskText, __instance.transform);
            infoOverlayRules.fontSize = infoOverlayRules.fontSizeMin = infoOverlayRules.fontSizeMax = 1.15f;
            infoOverlayRules.autoSizeTextContainer = false;
            infoOverlayRules.enableWordWrapping = false;
            infoOverlayRules.alignment = TextAlignmentOptions.TopLeft;
            infoOverlayRules.transform.position = Vector3.zero;
            infoOverlayRules.transform.localPosition = new(-3f, 1f, -910f);
            infoOverlayRules.transform.localScale = Vector3.one * 1.25f;
            infoOverlayRules.color = Palette.White;
            infoOverlayRules.enabled = false;
        }

        if (infoOverlayPlayer == null)
        {
            infoOverlayPlayer = Object.Instantiate(infoOverlayRules, __instance.transform);
            infoOverlayPlayer.maxVisibleLines = 28;
            infoOverlayPlayer.fontSize = infoOverlayPlayer.fontSizeMin = infoOverlayPlayer.fontSizeMax = 1.10f;
            infoOverlayPlayer.outlineWidth += 0.02f;
            infoOverlayPlayer.autoSizeTextContainer = false;
            infoOverlayPlayer.enableWordWrapping = false;
            infoOverlayPlayer.alignment = TextAlignmentOptions.Top;
            infoOverlayPlayer.transform.position = Vector3.zero;
            infoOverlayPlayer.transform.localPosition = new(0f, 1f, -910f);
            infoOverlayPlayer.transform.localScale = Vector3.one * 1.25f;
            infoOverlayPlayer.color = Palette.White;
            infoOverlayPlayer.enabled = false;
        }

        if (infoOverlayRoles == null)
        {
            infoOverlayRoles = Object.Instantiate(infoOverlayRules, __instance.transform);
            infoOverlayRoles.maxVisibleLines = 28;
            infoOverlayRoles.fontSize = infoOverlayRoles.fontSizeMin = infoOverlayRoles.fontSizeMax = 1f;
            infoOverlayRoles.outlineWidth += 0.02f;
            infoOverlayRoles.autoSizeTextContainer = false;
            infoOverlayRoles.enableWordWrapping = false;
            infoOverlayRoles.alignment = TextAlignmentOptions.TopLeft;
            infoOverlayRoles.transform.position = Vector3.zero;
            infoOverlayRoles.transform.localPosition = new(2.7f, 1f, -910f);
            infoOverlayRoles.transform.localScale = Vector3.one * 1.25f;
            infoOverlayRoles.color = Palette.White;
            infoOverlayRoles.enabled = false;
        }

        return true;
    }

    internal static void ShowInfoOverlay(HudManager __instance)
    {
        if (OverlayShown) return;

        if (PlayerControl.LocalPlayer == null || __instance == null) return;

        if (!Initialize(__instance)) return;

        MapBehaviour.Instance?.Close();

        if (MeetingHud.Instance != null) __instance.SetHudActive(false);

        OverlayShown = true;

        Transform parent;
        parent = __instance.transform;

        infoUnderlay.transform.parent = parent;
        infoOverlayRules.transform.parent = parent;
        infoOverlayPlayer.transform.parent = parent;
        infoOverlayRoles.transform.parent = parent;

        infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
        infoUnderlay.transform.localScale = new Vector3(9f, 5.3f, 1f);
        infoUnderlay.enabled = true;
        infoOverlayRules.enabled = true;
        infoOverlayPlayer.enabled = true;

        string rolesText = "";

        foreach (RoleInfo r in RoleInfoList.GetRoleInfoForPlayer(PlayerControl.LocalPlayer))
        {
            string roleOptions = r.RoleOptions;
            string roleDesc = r.FullDescription;
            rolesText += $"<size=150%>{r.ColorName}</size>{(roleDesc is not "" ? $"\n{r.FullDescription}" : "")}\n\n{(roleOptions != "" ? $"{roleOptions}\n\n" : "")}";
        }

        infoOverlayRoles.text = rolesText;
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started) infoOverlayRoles.enabled = true;
        else infoOverlayRoles.enabled = false;

        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
            infoOverlayRules.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            infoOverlayPlayer.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            infoOverlayRoles.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
        })));
    }

    internal static void HideInfoOverlay()
    {
        if (!OverlayShown) return;

        if (MeetingHud.Instance == null && ShipStatus.Instance != null) FastDestroyableSingleton<HudManager>.Instance.SetHudActive(true);

        OverlayShown = false;
        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            if (infoUnderlay != null)
            {
                infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                if (t >= 1.0f) infoUnderlay.enabled = false;
            }

            if (infoOverlayRules != null)
            {
                infoOverlayRules.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f) infoOverlayRules.enabled = false;
            }

            if (infoOverlayPlayer != null)
            {
                infoOverlayPlayer.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f) infoOverlayPlayer.enabled = false;
            }

            if (infoOverlayRoles != null)
            {
                infoOverlayRoles.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f) infoOverlayRoles.enabled = false;
            }
        })));
    }

    internal static void ToggleInfoOverlay(HudManager __instance)
    {
        if (OverlayShown) HideInfoOverlay();
        else ShowInfoOverlay(__instance);
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    internal static class CustomOverlayKeyInput
    {
        internal static void Postfix(KeyboardJoystick __instance)
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                ToggleInfoOverlay(FastDestroyableSingleton<HudManager>.Instance);
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    internal static class CustomOverlayUpdate
    {
        internal static void Postfix(HudManager __instance)
        {
            if (!Initialize(__instance)) return;
            if (!OverlayShown) return;
            if (PlayerControl.LocalPlayer == null || __instance == null) return;

            List<string> gameOptions = GameManager.Instance.LogicOptions.currentGameOptions.ToString().Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            infoOverlayRules.text = string.Join("\n", gameOptions);
            string PlayerText = ModResources.PlatformTitle;
            foreach (ClientData Client in AmongUsClient.Instance.allClients.ToArray())
            {
                if (Client == null) continue;
                if (Client.Character == null) continue;
                var player = Helpers.PlayerById(Client.Character.PlayerId);
                var Platform = $"{Client.PlatformData.Platform}";

                var PlayerName = Client.PlayerName.DeleteHTML();
                var HEXcolor = Helpers.GetColorHEX(Client);
                if (HEXcolor == "") HEXcolor = "FF000000";
                PlayerText += $"\n<color=#{HEXcolor}>■</color>{PlayerName} : {Platform.Replace("Standalone", "")}";
            }
            infoOverlayPlayer.text = PlayerText;
        }
    }

    internal class PlayerVersion
    {
        internal readonly Version version;
        internal readonly Guid guid;

        internal PlayerVersion(Version version, Guid guid)
        {
            this.version = version;
            this.guid = guid;
        }

        internal bool GuidMatches()
        {
            return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(guid);
        }
    }
}