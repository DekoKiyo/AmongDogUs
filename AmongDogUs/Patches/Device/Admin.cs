namespace AmongDogUs.Patches;

[Harmony]
internal static class AdminPatch
{
    internal static Dictionary<SystemTypes, List<Color>> playerColors = new();
    internal static float adminTimer = 0f;
    internal static TextMeshPro OutOfTime;
    internal static TextMeshPro TimeRemaining;
    internal static bool clearedIcons = false;
    internal static bool isUseAdmin = false;

    internal static void ResetData()
    {
        adminTimer = 0f;
        isUseAdmin = false;
        if (TimeRemaining != null)
        {
            Object.Destroy(TimeRemaining);
            TimeRemaining = null;
        }

        if (OutOfTime != null)
        {
            Object.Destroy(OutOfTime);
            OutOfTime = null;
        }
    }

    internal static void UseAdminTime()
    {
        // Don't waste network traffic if we're out of time.
        if (ModMapOptions.RestrictDevices > 0 && ModMapOptions.RestrictAdminTime > 0f && PlayerControl.LocalPlayer.IsAlive() && !PlayerControl.LocalPlayer.IsRole(RoleType.EvilHacker))
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UseAdminTime, SendOption.Reliable, -1);
            writer.Write(adminTimer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.UseAdminTime(adminTimer);
        }
        adminTimer = 0f;
    }

    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
    internal static class MapConsoleCanUsePatch
    {
        internal static bool Prefix(ref float __result, MapConsole __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            return true;
        }
    }

    [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
    internal static class MapConsoleUsePatch
    {
        internal static bool Prefix(MapConsole __instance)
        {
            return ModMapOptions.CanUseAdmin;
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
    internal static class MapCountOverlayOnEnablePatch
    {
        internal static void Prefix(MapCountOverlay __instance)
        {
            adminTimer = 0f;
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnDisable))]
    internal static class MapCountOverlayOnDisablePatch
    {
        internal static void Prefix(MapCountOverlay __instance)
        {
            UseAdminTime();
        }
    }

    [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
    internal static class MapCountOverlayUpdatePatch
    {
        internal static bool Prefix(MapCountOverlay __instance)
        {
            adminTimer += Time.deltaTime;
            if (adminTimer > 0.1f) UseAdminTime();

            // Save colors for the Hacker
            __instance.timer += Time.deltaTime;
            if (__instance.timer < 0.1f) return false;
            __instance.timer = 0f;

            playerColors = new Dictionary<SystemTypes, List<Color>>();

            if (ModMapOptions.RestrictDevices > 0)
            {
                if (OutOfTime == null)
                {
                    OutOfTime = Object.Instantiate(__instance.SabotageText, __instance.SabotageText.transform.parent);
                    OutOfTime.text = ModResources.RestrictOutOfTime;
                }

                if (TimeRemaining == null)
                {
                    TimeRemaining = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                    TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                    TimeRemaining.transform.position = Vector3.zero;
                    TimeRemaining.transform.localPosition = new Vector3(3.25f, 5.25f);
                    TimeRemaining.transform.localScale *= 2f;
                    TimeRemaining.color = Palette.White;
                }
                if (PlayerControl.LocalPlayer.IsRole(RoleType.EvilHacker))
                {
                    TimeRemaining.gameObject.SetActive(false);
                }
                else
                {
                    if (ModMapOptions.RestrictAdminTime <= 0f)
                    {
                        __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                        OutOfTime.gameObject.SetActive(true);
                        TimeRemaining.gameObject.SetActive(false);
                        isUseAdmin = false;
                        if (clearedIcons == false)
                        {
                            foreach (CounterArea ca in __instance.CountAreas) ca.UpdateCount(0);
                            clearedIcons = true;
                        }
                        return false;
                    }

                    clearedIcons = false;
                    OutOfTime.gameObject.SetActive(false);
                    string timeString = TimeSpan.FromSeconds(ModMapOptions.RestrictAdminTime).ToString(@"mm\:ss\.ff");
                    TimeRemaining.text = string.Format(ModResources.TimeRemaining, timeString);
                    // TimeRemaining.color = ModMapOptions.RestrictAdminTime > 10f ? Palette.AcceptedGreen : Palette.ImpostorRed;
                    isUseAdmin = true;
                    TimeRemaining.gameObject.SetActive(true);
                }
            }

            bool commsActive = false;
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                if (task.TaskType == TaskTypes.FixComms) commsActive = true;

            if (!__instance.isSab && commsActive)
            {
                __instance.isSab = true;
                __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                __instance.SabotageText.gameObject.SetActive(true);
                OutOfTime.gameObject.SetActive(false);
                return false;
            }

            if (__instance.isSab && !commsActive)
            {
                __instance.isSab = false;
                __instance.BackgroundColor.SetColor(Color.green);
                __instance.SabotageText.gameObject.SetActive(false);
                OutOfTime.gameObject.SetActive(false);
            }

            for (int i = 0; i < __instance.CountAreas.Length; i++)
            {
                CounterArea counterArea = __instance.CountAreas[i];
                List<Color> roomColors = new();
                playerColors.Add(counterArea.RoomType, roomColors);

                if (!commsActive)
                {
                    PlainShipRoom plainShipRoom = ShipStatus.Instance.FastRooms[counterArea.RoomType];

                    if (plainShipRoom != null && plainShipRoom.roomArea)
                    {
                        int Num = plainShipRoom.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                        int Num2 = Num;
                        // アプデ前アドミン仕様
                        if (CustomOptionsHolder.OldAirShipAdmin.GetBool() && (counterArea.RoomType is SystemTypes.Ventilation or SystemTypes.HallOfPortraits))
                        {
                            Num2 = 0;
                        }
                        for (int j = 0; j < Num; j++)
                        {
                            Collider2D collider2D = __instance.buffer[j];
                            if (!(collider2D.tag == "DeadBody"))
                            {
                                PlayerControl component = collider2D.GetComponent<PlayerControl>();
                                if (!component || component.Data == null || component.Data.Disconnected || component.Data.IsDead)
                                {
                                    Num2--;
                                }
                                else if (component?.cosmetics.currentBodySprite.BodySprite?.material != null)
                                {
                                    Color color = component.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
                                    /*
                                    if (Hacker.onlyColorType)
                                    {
                                        var id = Mathf.Max(0, Palette.PlayerColors.IndexOf(color));
                                        color = Helpers.isLighterColor((byte)id) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];
                                    }
                                    */
                                    roomColors.Add(color);
                                }
                            }
                            else
                            {
                                DeadBody component = collider2D.GetComponent<DeadBody>();
                                if (component)
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    if (playerInfo != null)
                                    {
                                        var color = Palette.PlayerColors[playerInfo.Object.CurrentOutfit.ColorId];
                                        /*if (Hacker.onlyColorType) color = Helpers.isLighterColor(playerInfo.Object.CurrentOutfit.ColorId) ? Palette.PlayerColors[7] : Palette.PlayerColors[6];*/
                                        roomColors.Add(color);
                                    }
                                }
                            }
                        }
                        counterArea.UpdateCount(Num2);
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't find counter for:" + counterArea.RoomType);
                    }
                }
                else
                {
                    counterArea.UpdateCount(0);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
    internal static class CounterAreaUpdateCountPatch
    {
        private static Material defaultMat;
        private static Material newMat;
        internal static void Postfix(CounterArea __instance)
        {
            // Hacker display saved colors on the admin panel
            // bool showHackerInfo = Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && Hacker.hackerTimer > 0;
            if (playerColors.ContainsKey(__instance.RoomType))
            {
                List<Color> colors = playerColors[__instance.RoomType];
                List<Color> impostorColors = new();
                List<Color> deadBodyColors = new();
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    // var color = p.myRend.material.GetColor("_BodyColor");
                    var color = Palette.PlayerColors[p.Data.DefaultOutfit.ColorId];
                    if (p.IsDead()) deadBodyColors.Add(color);
                }

                for (int i = 0; i < __instance.myIcons.Count; i++)
                {
                    PoolableBehavior icon = __instance.myIcons[i];
                    SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();

                    if (renderer != null)
                    {
                        if (defaultMat == null) defaultMat = renderer.material;
                        if (newMat == null) newMat = Object.Instantiate(defaultMat);
                        /*
                            if (showHackerInfo && colors.Count > i)
                            {
                                renderer.material = newMat;
                                var color = colors[i];
                                renderer.material.SetColor("_BodyColor", color);
                                var id = Palette.PlayerColors.IndexOf(color);
                                if (id < 0)
                                {
                                    renderer.material.SetColor("_BackColor", color);
                                }
                                else
                                {
                                    renderer.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                                }
                                renderer.material.SetColor("_VisorColor", Palette.VisorColor);
                            }
                        */
                        if (PlayerControl.LocalPlayer.IsRole(RoleType.EvilHacker) && EvilHacker.CanHasBetterAdmin)
                        {
                            renderer.material = newMat;
                            var color = colors[i];
                            if (impostorColors.Contains(color))
                            {
                                color = Palette.ImpostorRed;
                                renderer.material.SetColor("_BodyColor", color);
                                var id = Palette.PlayerColors.IndexOf(color);
                                if (id < 0) renderer.material.SetColor("_BackColor", color);
                                else renderer.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                                renderer.material.SetColor("_VisorColor", Palette.VisorColor);
                            }
                            else if (deadBodyColors.Contains(color))
                            {
                                color = Palette.Black;
                                renderer.material.SetColor("_BodyColor", color);
                                var id = Palette.PlayerColors.IndexOf(color);
                                if (id < 0) renderer.material.SetColor("_BackColor", color);
                                else renderer.material.SetColor("_BackColor", Palette.ShadowColors[id]);
                                renderer.material.SetColor("_VisorColor", Palette.VisorColor);
                            }
                            else renderer.material = defaultMat;
                        }
                        else renderer.material = defaultMat;
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
internal static class DisableRecordsAdminPatch
{
    internal static void Postfix()
    {
        if (!CustomOptionsHolder.EnableRecordsAdmin.GetBool() && GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
        {
            Transform recordsAdmin = GameObject.Find("Airship(Clone)").transform.FindChild("Records").FindChild("records_admin_map");
            Object.Destroy(recordsAdmin.gameObject);
        }
        if (!CustomOptionsHolder.EnableCockpitAdmin.GetBool() && GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
        {
            Transform cockpitAdmin = GameObject.Find("Airship(Clone)").transform.FindChild("Cockpit").FindChild("panel_cockpit_map");
            Object.Destroy(cockpitAdmin.gameObject);
        }
    }
}