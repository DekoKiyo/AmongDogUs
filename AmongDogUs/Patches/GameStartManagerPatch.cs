namespace AmongDogUs.Patches;

internal class GameStartManagerPatch
{
    internal static Dictionary<int, PlayerVersion> playerVersions = new();
    private static float timer = 600f;
    private static float kickingTimer = 0f;
    private static bool versionSent = false;

    [HarmonyPatch(typeof(AmongUsClient))]
    internal class AmongUsClientOnPlayerJoinedPatch
    {
        [HarmonyPatch(nameof(AmongUsClient.OnPlayerJoined)), HarmonyPostfix]
        internal static void ShareGameVersion()
        {
            if (PlayerControl.LocalPlayer != null)
            {
                Helpers.ShareGameVersion();
            }
        }
    }

    [HarmonyPatch(typeof(GameStartManager))]
    internal class GameStartManagerStartPatch
    {
        [HarmonyPatch(nameof(GameStartManager.Start)), HarmonyPostfix]
        internal static void Reset(GameStartManager __instance)
        {
            // Trigger version refresh
            versionSent = false;
            // Reset lobby countdown timer
            timer = 600f;
            // Reset kicking timer
            kickingTimer = 0f;
            // Copy lobby code
            string code = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            GUIUtility.systemCopyBuffer = code;
        }
    }

    [HarmonyPatch(typeof(GameStartManager))]
    internal class GameStartManagerUpdatePatch
    {
        private static bool update = false;
        private static string currentText = "";

        [HarmonyPatch(nameof(GameStartManager.Update)), HarmonyPrefix]
        internal static void Prefix(GameStartManager __instance)
        {
            // Lobby code
            if (DataManager.Settings.Gameplay.StreamerMode)
            {
                __instance.GameRoomNameCode.color = new(89, 162, 243);
                __instance.GameRoomNameCode.text = Main.RoomCodeText.Value;
            }
            else
            {
                __instance.GameRoomNameCode.text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            }

            if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;

            // カウントダウンキャンセル
            if (Input.GetKeyDown(KeyCode.LeftShift) && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
                GameStartManager.Instance.ResetStartState();
            // 即スタート
            if (Input.GetKeyDown(KeyCode.KeypadEnter) && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
                GameStartManager.Instance.countDownTimer = 0;
        }

        [HarmonyPatch(nameof(GameStartManager.Update)), HarmonyPostfix]
        internal static void Postfix(GameStartManager __instance)
        {
            string ColorCode = "ffffff";

            if (PlayerControl.LocalPlayer != null && !versionSent)
            {
                versionSent = true;
                Helpers.ShareGameVersion();
            }

            if (AmongUsClient.Instance.AmHost)
            {
                bool BlockGameStart = false;
                string message = "";
                foreach (ClientData client in AmongUsClient.Instance.allClients.ToArray())
                {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled) continue;
                    else if (!playerVersions.ContainsKey(client.Id))
                    {
                        BlockGameStart = true;
                        message += $"<color=#00a2ff>{string.Format(ModResources.ErrorNotInstalled, client.Character.Data.PlayerName)}\n</color>";
                    }
                    else
                    {
                        PlayerVersion PV = playerVersions[client.Id];
                        int diff = Main.Version.CompareTo(PV.version);
                        if (diff > 0)
                        {
                            message += $"<color=#00a2ff>{string.Format(ModResources.ErrorOlderVersion, client.Character.Data.PlayerName)} (Version{playerVersions[client.Id].version})\n</color>";
                            BlockGameStart = true;
                        }
                        else if (diff < 0)
                        {
                            message += $"<color=#00a2ff>{string.Format(ModResources.ErrorNewerVersion, client.Character.Data.PlayerName)} (Version{playerVersions[client.Id].version})\n</color>";
                            BlockGameStart = true;
                        }
                        else if (!PV.GuidMatches())
                        {
                            // version presumably matches, check if Guid matches
                            message += $"<color=#00a2ff>{string.Format(ModResources.ErrorWrongVersion, client.Character.Data.PlayerName)} Version{playerVersions[client.Id].version} <size=30%>({PV.guid.ToString()})</size>\n</color>";
                            BlockGameStart = true;
                        }
                    }
                }

                if (BlockGameStart)
                {
                    __instance.StartButton.color = __instance.startLabelText.color = Palette.DisabledClear;
                    __instance.GameStartText.text = message;
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                }
                else
                {
                    __instance.StartButton.color = __instance.startLabelText.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                }
            }

            // Client update with handshake infos
            if (!AmongUsClient.Instance.AmHost)
            {
                if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId) || Main.Version.CompareTo(playerVersions[AmongUsClient.Instance.HostId].version) != 0)
                {
                    kickingTimer += Time.deltaTime;
                    if (kickingTimer > 10)
                    {
                        kickingTimer = 0;
                        AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                        SceneChanger.ChangeScene("MainMenu");
                    }

                    __instance.GameStartText.text = string.Format(ModResources.ErrorHostNoVersion, Math.Round(10 - kickingTimer));
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                }
                else
                {
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    if (__instance.startState != GameStartManager.StartingStates.Countdown)
                    {
                        __instance.GameStartText.text = string.Empty;
                    }
                }
            }

            // Lobby timer
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance

            if (update) currentText = __instance.PlayerCounter.text;

            timer = Mathf.Max(0f, timer -= Time.deltaTime);
            int minutes = (int)timer / 60;
            int seconds = (int)timer % 60;
            // string suffix = $" ({minutes:00}:{seconds:00})";

            switch (minutes)
            {
                case <= 02:
                    ColorCode = "d20000";
                    break;
                case <= 05:
                    ColorCode = "ffff00";
                    break;
                case <= 10:
                    ColorCode = "00e300";
                    break;
            }
            string suffix = $" <color=#{ColorCode}>\n({minutes:00}:{seconds:00})</color>";

            __instance.PlayerCounter.text = currentText + suffix;
            __instance.PlayerCounter.autoSizeTextContainer = true;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    internal class GameStartManagerBeginGame
    {
        internal static bool Prefix(GameStartManager __instance)
        {
            // Block game start if not everyone has the same mod version
            bool continueStart = true;

            if (AmongUsClient.Instance.AmHost)
            {
                foreach (ClientData client in AmongUsClient.Instance.allClients.GetFastEnumerator())
                {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled) continue;

                    if (!playerVersions.ContainsKey(client.Id))
                    {
                        continueStart = false;
                        break;
                    }

                    PlayerVersion PV = playerVersions[client.Id];
                    int diff = Main.Version.CompareTo(PV.version);
                    if (diff != 0 || !PV.GuidMatches())
                    {
                        continueStart = false;
                        break;
                    }
                }

                if (CustomOptionsHolder.RandomMap.GetBool() && continueStart)
                {
                    // 0 = Skeld
                    // 1 = Mira HQ
                    // 2 = Polus
                    // 3 = Airship
                    // 4 = Submerged
                    List<EMap> possibleMaps = new();
                    if (CustomOptionsHolder.RandomMapEnableSkeld.GetBool()) possibleMaps.Add(EMap.Skeld);
                    if (CustomOptionsHolder.RandomMapEnableMira.GetBool()) possibleMaps.Add(EMap.Mira);
                    if (CustomOptionsHolder.RandomMapEnablePolus.GetBool()) possibleMaps.Add(EMap.Polus);
                    if (CustomOptionsHolder.RandomMapEnableAirShip.GetBool()) possibleMaps.Add(EMap.AirShip);
                    if (CustomOptionsHolder.RandomMapEnableSubmerged.GetBool()) possibleMaps.Add(EMap.Submerged);

                    if (possibleMaps.Count > 0)
                    {
                        var chosenMapId = (byte)possibleMaps[Main.Random.Next(possibleMaps.Count)];
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.DynamicMapOption, SendOption.Reliable, -1);
                        writer.Write(chosenMapId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.DynamicMapOption(chosenMapId);
                    }
                }
            }
            return continueStart;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.SetStartCounter))]
    internal static class SetStartCounterPatch
    {
        internal static void Postfix(GameStartManager __instance, sbyte sec)
        {
            if (sec > 0) __instance.startState = GameStartManager.StartingStates.Countdown;
            if (sec <= 0) __instance.startState = GameStartManager.StartingStates.NotStarting;
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

        internal bool GuidMatches() => Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(guid);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
internal static class SetAnonymousVotesPatch
{
    internal static void Postfix()
    {
        GameManager.Instance.LogicOptions.currentGameOptions.SetBool(BoolOptionNames.AnonymousVotes, true);
    }
}