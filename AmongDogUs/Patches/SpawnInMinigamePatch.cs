// Source code from TheOtherRolesGMH

namespace AmongDogUs.Patches;

[HarmonyPatch]
internal static class SpawnInMinigamePatch
{
    private static PassiveButton selected = null;
    internal static SynchronizeData synchronizeData = new();
    internal static bool isFirstSpawn = true;
    internal static float InitialDoorCooldown { get { return CustomOptionsHolder.AirshipInitialDoorCooldown.GetFloat(); } }
    internal static float InitialSabotageCooldown { get { return CustomOptionsHolder.AirshipInitialSabotageCooldown.GetFloat(); } }

    internal static void Reset()
    {
        isFirstSpawn = true;
    }

    private static void ResetButtons()
    {
        // MapUtilities.CachedShipStatus.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>().ForceSabTime(10f);
        isFirstSpawn = false;
        if (CustomOptionsHolder.AirshipSetOriginalCooldown.GetBool())
        {
            CachedPlayer.LocalPlayer.PlayerControl.SetKillTimerUnchecked(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            foreach (var b in CustomButton.buttons)
            {
                b.Timer = b.MaxTimer;
            }
        }
        else
        {
            CachedPlayer.LocalPlayer.PlayerControl.SetKillTimerUnchecked(10f);
            CustomButton.buttons.ForEach(x => x.Timer = 10f);
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin)), HarmonyPrefix]
    internal static bool Prefix(SpawnInMinigame __instance, PlayerTask task)
    {
        CustomButton.StopCountdown = true;
        // base.Begin(task);
        __instance.MyTask = task;
        __instance.MyNormTask = task as NormalPlayerTask;
        if (CachedPlayer.LocalPlayer.PlayerControl)
        {
            if (MapBehaviour.Instance) MapBehaviour.Instance.Close();
            CachedPlayer.LocalPlayer.PlayerControl.NetTransform.Halt();
        }
        __instance.StartCoroutine(__instance.CoAnimateOpen());

        List<SpawnInMinigame.SpawnLocation> list = __instance.Locations.ToList();

        SpawnInMinigame.SpawnLocation[] array = list.ToArray<SpawnInMinigame.SpawnLocation>();
        array.Shuffle(0);
        array = (from s in array.Take(__instance.LocationButtons.Length)
                 orderby s.Location.x, s.Location.y descending
                 select s).ToArray();
        CachedPlayer.LocalPlayer.PlayerControl.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));

        for (int i = 0; i < __instance.LocationButtons.Length; i++)
        {
            PassiveButton passiveButton = __instance.LocationButtons[i];
            SpawnInMinigame.SpawnLocation pt = array[i];
            passiveButton.OnClick.AddListener((UnityAction)(() => SpawnAt(__instance, pt.Location)));
            passiveButton.GetComponent<SpriteAnim>().Stop();
            passiveButton.GetComponent<SpriteRenderer>().sprite = pt.Image;
            // passiveButton.GetComponentInChildren<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, Array.Empty<object>());
            passiveButton.GetComponentInChildren<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            ButtonAnimRolloverHandler component = passiveButton.GetComponent<ButtonAnimRolloverHandler>();
            component.StaticOutImage = pt.Image;
            component.RolloverAnim = pt.Rollover;
            component.HoverSound = pt.RolloverSfx ? pt.RolloverSfx : __instance.DefaultRolloverSound;
        }

        CachedPlayer.LocalPlayer.PlayerControl.gameObject.SetActive(false);
        CachedPlayer.LocalPlayer.PlayerControl.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));
        __instance.StartCoroutine(__instance.RunTimer());
        ControllerManager.Instance.OpenOverlayMenu(__instance.name, null, __instance.DefaultButtonSelected, __instance.ControllerSelectable, false);
        PlayerControl.HideCursorTemporarily();
        ConsoleJoystick.SetMode_Menu();
        return false;
    }

    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin)), HarmonyPostfix]
    internal static void Postfix(SpawnInMinigame __instance)
    {
        selected = null;

        if (!CustomOptionsHolder.AirshipSynchronizedSpawning.GetBool()) return;

        foreach (var button in __instance.LocationButtons)
        {
            button.OnClick.AddListener((UnityAction)(() =>
            {
                if (selected == null) selected = button;
            }
            ));
        }
    }

    [HarmonyPatch(typeof(SpawnInMinigame._RunTimer_d__10), nameof(SpawnInMinigame._RunTimer_d__10.MoveNext)), HarmonyPostfix]
    internal static void Postfix(SpawnInMinigame._RunTimer_d__10 __instance)
    {
        if (!CustomOptionsHolder.AirshipSynchronizedSpawning.GetBool()) return;
        if (selected != null) __instance.__4__this.Text.text = ModResources.AirShipWaitingOtherPlayer;
    }

    internal static void SpawnAt(SpawnInMinigame __instance, Vector3 spawnAt)
    {
        if (!CustomOptionsHolder.AirshipSynchronizedSpawning.GetBool())
        {
            if (isFirstSpawn) ResetButtons();
            CustomButton.StopCountdown = false;

            if (__instance.amClosing != Minigame.CloseState.None) return;

            __instance.gotButton = true;
            CachedPlayer.LocalPlayer.PlayerControl.gameObject.SetActive(true);
            __instance.StopAllCoroutines();
            CachedPlayer.LocalPlayer.PlayerControl.NetTransform.RpcSnapTo(spawnAt);
            FastDestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
            __instance.Close();
        }
        else
        {
            SynchronizeData.Synchronize(SynchronizeTag.PreSpawnMinigame, CachedPlayer.LocalPlayer.PlayerControl.PlayerId);

            if (__instance.amClosing != Minigame.CloseState.None) return;
            if (__instance.gotButton) return;

            __instance.gotButton = true;

            foreach (var button in __instance.LocationButtons) button.enabled = false;

            __instance.StartCoroutine(Effects.Lerp(10f, new Action<float>((p) =>
            {
                float time = p * 10f;

                foreach (var button in __instance.LocationButtons)
                {
                    if (selected == button)
                    {
                        if (time > 0.3f)
                        {
                            float x = button.transform.localPosition.x;
                            if (x < 0f) x += 10f * Time.deltaTime;
                            if (x > 0f) x -= 10f * Time.deltaTime;
                            if (Mathf.Abs(x) < 10f * Time.deltaTime) x = 0f;
                            button.transform.localPosition = new Vector3(x, button.transform.localPosition.y, button.transform.localPosition.z);
                        }
                    }
                    else
                    {
                        var color = button.GetComponent<SpriteRenderer>().color;
                        float a = color.a;
                        if (a > 0f) a -= 2f * Time.deltaTime;
                        if (a < 0f) a = 0f;
                        button.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, a);
                        button.GetComponentInChildren<TextMeshPro>().color = new Color(1f, 1f, 1f, a);
                    }

                    if (__instance.amClosing != Minigame.CloseState.None) return;

                    if (synchronizeData.Align(SynchronizeTag.PreSpawnMinigame, false) || p == 1f)
                    {
                        CachedPlayer.LocalPlayer.PlayerControl.gameObject.SetActive(true);
                        __instance.StopAllCoroutines();
                        CachedPlayer.LocalPlayer.PlayerControl.NetTransform.RpcSnapTo(spawnAt);
                        FastDestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
                        synchronizeData.Reset(SynchronizeTag.PreSpawnMinigame);
                        __instance.Close();
                        CustomButton.StopCountdown = false;

                        // サボタージュのクールダウンをリセット
                        SabotageSystemType saboSystem = MapUtilities.CachedShipStatus.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
                        AccessTools.PropertySetter(typeof(SabotageSystemType), "IsDirty").Invoke(saboSystem, new object[] { true });
                        saboSystem.ForceSabTime(0f);
                        saboSystem.Timer = InitialSabotageCooldown;
                        DoorsSystemType doorSystem = MapUtilities.CachedShipStatus.Systems[SystemTypes.Doors].Cast<DoorsSystemType>();
                        AccessTools.PropertySetter(typeof(DoorsSystemType), "IsDirty").Invoke(doorSystem, new object[] { true });
                        doorSystem.timers[SystemTypes.MainHall] = InitialDoorCooldown;
                        doorSystem.timers[SystemTypes.Brig] = InitialDoorCooldown;
                        doorSystem.timers[SystemTypes.Comms] = InitialDoorCooldown;
                        doorSystem.timers[SystemTypes.Medical] = InitialDoorCooldown;
                        doorSystem.timers[SystemTypes.Engine] = InitialDoorCooldown;
                        doorSystem.timers[SystemTypes.Records] = InitialDoorCooldown;
                        doorSystem.timers[SystemTypes.Kitchen] = InitialDoorCooldown;

                        if (isFirstSpawn) ResetButtons();
                    }
                }
            })));
            return;
        }
    }
}