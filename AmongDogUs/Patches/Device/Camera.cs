namespace AmongDogUs.Patches;

[Harmony]
internal static class CameraPatch
{
    internal static float cameraTimer = 0f;

    internal static void ResetData()
    {
        cameraTimer = 0f;
        SurveillanceMinigamePatch.ResetData();
        PlanetSurveillanceMinigamePatch.ResetData();
    }

    internal static void UseCameraTime()
    {
        // Don't waste network traffic if we're out of time.
        if (ModMapOptions.RestrictDevices > 0 && ModMapOptions.RestrictCamerasTime > 0f && PlayerControl.LocalPlayer.IsAlive())
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UseCameraTime, SendOption.Reliable, -1);
            writer.Write(cameraTimer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.UseCameraTime(cameraTimer);
        }
        cameraTimer = 0f;
    }

    [HarmonyPatch]
    internal static class SurveillanceMinigamePatch
    {
        private static int page = 0;
        private static float timer = 0f;
        private static TextMeshPro TimeRemaining;

        internal static void ResetData()
        {
            if (TimeRemaining != null)
            {
                Object.Destroy(TimeRemaining);
                TimeRemaining = null;
            }
        }

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Begin))]
        internal static class SurveillanceMinigameBeginPatch
        {
            internal static void Prefix(SurveillanceMinigame __instance)
            {
                cameraTimer = 0f;
            }

            internal static void Postfix(SurveillanceMinigame __instance)
            {
                // Add securityGuard cameras
                page = 0;
                timer = 0;
                if (ShipStatus.Instance.AllCameras.Length > 4 && __instance.FilteredRooms.Length > 0)
                {
                    __instance.textures = __instance.textures.ToList().Concat(new RenderTexture[ShipStatus.Instance.AllCameras.Length - 4]).ToArray();
                    for (int i = 4; i < ShipStatus.Instance.AllCameras.Length; i++)
                    {
                        SurvCamera surv = ShipStatus.Instance.AllCameras[i];
                        Camera camera = Object.Instantiate(__instance.CameraPrefab);
                        camera.transform.SetParent(__instance.transform);
                        camera.transform.position = new Vector3(surv.transform.position.x, surv.transform.position.y, 8f);
                        camera.orthographicSize = 2.35f;
                        RenderTexture temporary = RenderTexture.GetTemporary(256, 256, 16, (RenderTextureFormat)0);
                        __instance.textures[i] = temporary;
                        camera.targetTexture = temporary;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
        internal static class SurveillanceMinigameUpdatePatch
        {
            internal static bool Prefix(SurveillanceMinigame __instance)
            {
                cameraTimer += Time.deltaTime;
                if (cameraTimer > 0.1f) UseCameraTime();
                if (ModMapOptions.RestrictDevices > 0)
                {
                    if (TimeRemaining == null)
                    {
                        TimeRemaining = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                        TimeRemaining.alignment = TextAlignmentOptions.Center;
                        TimeRemaining.transform.position = Vector3.zero;
                        TimeRemaining.transform.localPosition = new(0.0f, -1.7f);
                        TimeRemaining.transform.localScale *= 1.8f;
                        TimeRemaining.color = Palette.White;
                    }
                    if (ModMapOptions.RestrictCamerasTime <= 0f)
                    {
                        __instance.Close();
                        return false;
                    }
                    string timeString = TimeSpan.FromSeconds(ModMapOptions.RestrictCamerasTime).ToString(@"mm\:ss\.ff");
                    TimeRemaining.text = string.Format(ModResources.TimeRemaining, timeString);
                    TimeRemaining.gameObject.SetActive(true);
                }
                // Update normal and securityGuard cameras
                timer += Time.deltaTime;
                int NumberOfPages = Mathf.CeilToInt(ShipStatus.Instance.AllCameras.Length / 4f);

                bool update = false;

                if (timer > 3f || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    update = true;
                    timer = 0f;
                    page = (page + 1) % NumberOfPages;
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    page = (page + NumberOfPages - 1) % NumberOfPages;
                    update = true;
                    timer = 0f;
                }

                if ((__instance.isStatic || update) && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
                {
                    __instance.isStatic = false;
                    for (int i = 0; i < __instance.ViewPorts.Length; i++)
                    {
                        __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                        __instance.SabText[i].gameObject.SetActive(false);
                        if (page * 4 + i < __instance.textures.Length) __instance.ViewPorts[i].material.SetTexture("_MainTex", __instance.textures[page * 4 + i]);
                        else __instance.ViewPorts[i].sharedMaterial = __instance.StaticMaterial;
                    }
                }
                else if (!__instance.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
                {
                    __instance.isStatic = true;
                    for (int j = 0; j < __instance.ViewPorts.Length; j++)
                    {
                        __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                        __instance.SabText[j].gameObject.SetActive(true);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Close))]
        internal static class SurveillanceMinigameClosePatch
        {
            internal static void Prefix(SurveillanceMinigame __instance)
            {
                UseCameraTime();
            }
        }
    }

    [HarmonyPatch]
    internal static class PlanetSurveillanceMinigamePatch
    {
        private static TextMeshPro TimeRemaining;

        internal static void ResetData()
        {
            if (TimeRemaining != null)
            {
                Object.Destroy(TimeRemaining);
                TimeRemaining = null;
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Begin))]
        internal static class PlanetSurveillanceMinigameBeginPatch
        {
            internal static void Prefix(PlanetSurveillanceMinigame __instance)
            {
                cameraTimer = 0f;
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
        internal static class PlanetSurveillanceMinigameUpdatePatch
        {
            internal static bool Prefix(PlanetSurveillanceMinigame __instance)
            {
                cameraTimer += Time.deltaTime;
                if (cameraTimer > 0.1f) UseCameraTime();
                if (ModMapOptions.RestrictDevices > 0)
                {
                    if (TimeRemaining == null)
                    {
                        TimeRemaining = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                        TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                        TimeRemaining.transform.position = Vector3.zero;
                        TimeRemaining.transform.localPosition = new Vector3(0.95f, 4.45f);
                        TimeRemaining.transform.localScale *= 1.8f;
                        TimeRemaining.color = Palette.White;
                    }
                    if (ModMapOptions.RestrictCamerasTime <= 0f)
                    {
                        __instance.Close();
                        return false;
                    }
                    string timeString = TimeSpan.FromSeconds(ModMapOptions.RestrictCamerasTime).ToString(@"mm\:ss\.ff");
                    TimeRemaining.text = string.Format(ModResources.TimeRemaining, timeString);
                    TimeRemaining.gameObject.SetActive(true);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Close))]
        internal static class PlanetSurveillanceMinigameClosePatch
        {
            internal static void Prefix(PlanetSurveillanceMinigame __instance)
            {
                UseCameraTime();
            }
        }
    }

    [HarmonyPatch]
    internal static class DoorLogPatch
    {
        private static TextMeshPro TimeRemaining;

        internal static void ResetData()
        {
            if (TimeRemaining != null)
            {
                Object.Destroy(TimeRemaining);
                TimeRemaining = null;
            }
        }

        [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
        internal static class SecurityLogGameBeginPatch
        {
            internal static void Prefix(Minigame __instance)
            {
                if (__instance is SecurityLogGame)
                    cameraTimer = 0f;
            }
        }

        [HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
        internal static class SecurityLogGameUpdatePatch
        {
            internal static bool Prefix(SecurityLogGame __instance)
            {
                cameraTimer += Time.deltaTime;
                if (cameraTimer > 0.1f) UseCameraTime();
                if (ModMapOptions.RestrictDevices > 0)
                {
                    if (TimeRemaining == null)
                    {
                        TimeRemaining = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                        TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                        TimeRemaining.transform.position = Vector3.zero;
                        TimeRemaining.transform.localPosition = new Vector3(1.0f, 4.25f);
                        TimeRemaining.transform.localScale *= 1.6f;
                        TimeRemaining.color = Palette.White;
                    }
                    if (ModMapOptions.RestrictCamerasTime <= 0f)
                    {
                        __instance.Close();
                        return false;
                    }
                    string timeString = TimeSpan.FromSeconds(ModMapOptions.RestrictCamerasTime).ToString(@"mm\:ss\.ff");
                    TimeRemaining.text = string.Format(ModResources.TimeRemaining, timeString);
                    TimeRemaining.gameObject.SetActive(true);
                }
                return true;
            }
        }
    }
}