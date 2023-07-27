// Source code from TheOtherRolesGMH
// Edited by DekoKiyo

namespace AmongDogUs.Patches;

[HarmonyPatch]
internal static class SyncMeeting
{
    internal static SynchronizeData synchronizeData = new();
    internal static bool IsShowingKillAnimation = false;
    internal static OverlayKillAnimation currentOverlayKillAnimation = null;
    internal static MeetingCalledAnimation currentMeetingCalledAnimation = null;

    internal static void StartMeeting()
    {
        CustomOverlays.ShowBlackBG();
        CustomOverlays.HideInfoOverlay();
        AmongDogUs.OnMeetingStart();
    }

    internal static void PopulateButtons(MeetingHud __instance, byte reporter)
    {
        // 会議に参加しないPlayerControlを持つRoleが増えたらこのListに追加
        // 特殊なplayerInfo.Role.Roleを設定することで自動的に無視できないか？もしくはフラグをplayerInfoのどこかに追加
        var playerControlsToBeIgnored = new List<PlayerControl>() { };
        playerControlsToBeIgnored.RemoveAll(x => x == null);
        var playerIdsToBeIgnored = playerControlsToBeIgnored.Select(x => x.PlayerId);
        // Generate PlayerVoteAreas
        __instance.playerStates = new PlayerVoteArea[GameData.Instance.PlayerCount - playerIdsToBeIgnored.Count()];
        int playerStatesCounter = 0;
        for (int i = 0; i < __instance.playerStates.Length + playerIdsToBeIgnored.Count(); i++)
        {
            if (playerIdsToBeIgnored.Contains(GameData.Instance.AllPlayers[i].PlayerId)) continue;
            GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
            PlayerVoteArea playerVoteArea = __instance.playerStates[playerStatesCounter] = __instance.CreateButton(playerInfo);
            playerVoteArea.Parent = __instance;
            playerVoteArea.SetTargetPlayerId(playerInfo.PlayerId);
            playerVoteArea.SetDead(reporter == playerInfo.PlayerId, playerInfo.Disconnected || playerInfo.IsDead, playerInfo.Role.Role == RoleTypes.GuardianAngel);
            playerVoteArea.UpdateOverlay();
            playerStatesCounter++;
        }
        foreach (PlayerVoteArea playerVoteArea2 in __instance.playerStates)
        {
            ControllerManager.Instance.AddSelectableUiElement(playerVoteArea2.PlayerButton, false);
        }
        __instance.SortButtons();
    }

    private static IEnumerator CoStartMeeting(PlayerControl reporter, GameData.PlayerInfo target)
    {
        // 既存処理の移植
        {
            while (!MeetingHud.Instance) yield return null;

            MeetingRoomManager.Instance.RemoveSelf();
            for (int i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
            {
                PlayerControl playerControl = PlayerControl.AllPlayerControls[i];
                playerControl?.ResetForMeeting();
            }

            if (MapBehaviour.Instance) MapBehaviour.Instance.Close();
            if (Minigame.Instance) Minigame.Instance.ForceClose();

            MapUtilities.CachedShipStatus.OnMeetingCalled();
            KillAnimation.SetMovement(reporter, true);
        }

        // 遅延処理追加そのままyield returnで待ちを入れるとロックしたのでHudManagerのコルーチンとして実行させる
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(CoStartMeeting2(reporter, target).WrapToIl2Cpp());
        yield break;
    }

    private static float Delay { get { return CustomOptionsHolder.DelayBeforeMeeting.GetFloat(); } }

    private static IEnumerator CoStartMeeting2(PlayerControl reporter, GameData.PlayerInfo target)
    {
        // Modで追加する遅延処理
        {
            // ボタンと同時に通報が入った場合のバグ対応、他のクライアントからキルイベントが飛んでくるのを待つ
            // 見えては行けないものが見えるので暗転させる
            MeetingHud.Instance.state = MeetingHud.VoteStates.Animating; // ゲッサーのキル用meetingUpdateが呼ばれないようにするおまじない（呼ばれるとバグる）
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            var blackScreen = Object.Instantiate(hudManager.FullScreen, hudManager.transform);
            blackScreen.color = Palette.Black;
            blackScreen.transform.position = Vector3.zero;
            blackScreen.transform.localPosition = new(0f, 0f, -910f);
            blackScreen.transform.localScale = new(10f, 10f, 1f);
            blackScreen.gameObject.SetActive(true);
            blackScreen.enabled = true;

            var grayScreen = Object.Instantiate(hudManager.FullScreen, hudManager.transform);
            grayScreen.color = Palette.Black;
            grayScreen.transform.position = Vector3.zero;
            grayScreen.transform.localPosition = new(0f, 0f, -920f);
            grayScreen.transform.localScale = new(10f, 10f, 1f);
            grayScreen.gameObject.SetActive(true);
            grayScreen.enabled = true;

            RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
            GameObject gameObject = Object.Instantiate(roomTracker.gameObject);
            Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
            gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
            gameObject.transform.localPosition = new(0, 0, -930f);
            gameObject.transform.localScale = Vector3.one * 5f;
            var text = gameObject.GetComponent<TMP_Text>();
            text.text = "waiting";
            if (text != null) text.color = Color.white;
            // if (!synchronizeData.Align(SynchronizeTag.SyncMeetingStart, true)) yield return null;
            yield return Effects.Lerp(Delay, new Action<float>((p) =>
            {
                // Delayed action
                grayScreen.color = new(1.0f, 1.0f, 1.0f, 0.5f - p / 2);
                string time = (Delay - (p * Delay)).ToString("0.00");
                if (time == "0") return;
                text.text = string.Format(ModResources.MeetingWaitingOtherPlayer, time);
                if (text != null) text.color = Color.white;
            }));
            // yield return new WaitForSeconds(2f);
            Object.Destroy(text.gameObject);
            Object.Destroy(blackScreen);
            Object.Destroy(grayScreen);

            // ミーティング画面の並び替えを直す
            PopulateButtons(MeetingHud.Instance, reporter.Data.PlayerId);
            // PopulateButtonsPostfix(MeetingHud.Instance);
        }

        // 既存処理の移植
        {
            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            GameData.PlayerInfo[] deadBodies = (from b in array select GameData.Instance.GetPlayerById(b.ParentId)).ToArray();
            for (int j = 0; j < array.Length; j++)
            {
                if (array[j] != null && array[j].gameObject != null)
                {
                    Object.Destroy(array[j].gameObject);
                }
                else
                {
                    Debug.LogError("Encountered a null Dead Body while destroying.");
                }
            }
            ShapeshifterEvidence[] array2 = Object.FindObjectsOfType<ShapeshifterEvidence>();
            for (int k = 0; k < array2.Length; k++)
            {
                if (array2[k] != null && array2[k].gameObject != null)
                {
                    Object.Destroy(array2[k].gameObject);
                }
                else
                {
                    Debug.LogError("Encountered a null Evidence while destroying.");
                }
            }
            Object.Destroy(FastDestroyableSingleton<HudManager>.Instance.KillOverlay);
            MeetingHud.Instance.StartCoroutine(MeetingHud.Instance.CoIntro(reporter.Data, target, deadBodies));
        }
        yield break;
    }

    private static void StartMeetingCoroutine(PlayerControl reporter, GameData.PlayerInfo target)
    {
        ShipStatus.Instance.StartCoroutine(CoStartMeeting(reporter, target).WrapToIl2Cpp());
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting)), HarmonyPrefix]
    internal static bool StartMeeting(PlayerControl __instance, GameData.PlayerInfo target)
    {
        if (CustomOptionsHolder.DelayBeforeMeeting.GetBool())
        {
            // SynchronizeData.Synchronize(SynchronizeTag.SyncMeetingStart, CachedPlayer.LocalPlayer.PlayerControl.PlayerId);

            // MOD追加処理
            {
                Main.Logger.LogInfo("ShipStatus.StartMeeting");
                StartMeeting();
                // Safe AntiTeleport positions
                // AntiTeleport.position = CachedPlayer.LocalPlayer.PlayerControl.transform.position;
                // Count meetings
                if (target == null) ModMapOptions.MeetingsCount++;
            }

            // 既存処理の移植
            {
                bool flag = target == null;
                Telemetry.Instance.WriteMeetingStarted(flag);
                StartMeetingCoroutine(__instance, target); // 変更部分
                if (__instance.AmOwner)
                {
                    if (flag)
                    {
                        __instance.RemainingEmergencies--;
                        StatsManager.Instance.IncrementStat(StringNames.StatsEmergenciesCalled);
                        return false;
                    }
                    StatsManager.Instance.IncrementStat(StringNames.StatsBodiesReported);
                }
            }
            return false;
        }
        return true;
    }
}