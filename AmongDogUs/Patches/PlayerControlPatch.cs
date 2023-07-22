namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(PlayerControl))]
internal static class PlayerControlPatch
{
    internal static void UpdateVentButtonVisibility(PlayerControl pc)
    {
        if (pc.AmOwner && Helpers.ShowButtons)
        {
            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Hide();
            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.Hide();

            if (Helpers.ShowButtons)
            {
                if (pc.RoleCanUseVents()) FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Show();

                if (pc.RoleCanSabotage())
                {
                    FastDestroyableSingleton<HudManager>.Instance.SabotageButton.Show();
                    FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(true);
                }
            }
        }
    }

    internal static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> unTargetablePlayers = null, PlayerControl targetingPlayer = null)
    {
        PlayerControl result = null;
        float Num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;

        unTargetablePlayers ??= new List<PlayerControl>();

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            GameData.PlayerInfo playerInfo = allPlayers[i];
            if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
            {
                PlayerControl @object = playerInfo.Object;
                // if that player is not targetable: skip check
                if (unTargetablePlayers.Any(x => x == @object)) continue;

                if (@object && (!@object.inVent || targetPlayersInVents))
                {
                    Vector2 vector = @object.GetTruePosition() - truePosition;
                    float magnitude = vector.magnitude;
                    if (magnitude <= Num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                    {
                        result = @object;
                        Num = magnitude;
                    }
                }
            }
        }
        return result;
    }

    internal static void SetPlayerOutline(PlayerControl target, Color color)
    {
        if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) return;

        target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
        target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
    }

    // Update functions
    internal static void SetBasePlayerOutlines()
    {
        foreach (PlayerControl target in PlayerControl.AllPlayerControls)
        {
            if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) continue;

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
        }
    }

    internal static void ImpostorSetTarget()
    {
        if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || !PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead)
        {
            // !isImpostor || !canMove || isDead
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            return;
        }

        PlayerControl target = SetTarget(true, true, new() { null });
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpostorRed);
    }

    internal static TextMeshPro meetingInfo = null;
    internal static TextMeshPro playerInfo = null;
    internal static Transform meetingInfoTransform = null;
    internal static Transform playerInfoTransform = null;

    internal static void UpdatePlayerInfo()
    {
        bool commsActive = false;
        foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
        {
            if (t.TaskType is TaskTypes.FixComms)
            {
                commsActive = true;
                break;
            }
        }

        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            if (p is null) continue;

            // Colorblind Text During the round
            if (p.cosmetics.colorBlindText is not null && p.cosmetics.showColorBlindText && p.cosmetics.colorBlindText.gameObject.active)
            {
                p.cosmetics.colorBlindText.transform.localPosition = new(0, -1f, 0f);
            }

            p.cosmetics.nameText.transform.parent.SetLocalZ(-0.0001f); // This moves both the name AND the color blind text behind objects (if the player is behind the object), like the rock on polus

            if (p == CachedPlayer.LocalPlayer.PlayerControl || CachedPlayer.LocalPlayer.Data.IsDead)
            {
                playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                if (playerInfoTransform.IsTransformNull())
                {
                    playerInfo = Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                    playerInfo.transform.localPosition += Vector3.up * 0.225f;
                    playerInfo.fontSize *= 0.75f;
                    playerInfo.gameObject.name = "Info";
                    playerInfo.color = playerInfo.color.SetAlpha(1f);
                }
                else playerInfo = playerInfoTransform.GetComponent<TextMeshPro>();

                if (Helpers.ShowMeetingText)
                {
                    PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                    if (playerVoteArea is not null)
                    {
                        meetingInfoTransform = playerVoteArea.NameText.transform.parent.FindChild("Info");
                        if (meetingInfoTransform.IsTransformNull())
                        {
                            meetingInfo = Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                            meetingInfo.transform.localPosition += Vector3.up * 0.2f;
                            meetingInfo.fontSize *= 0.60f;
                            meetingInfo.gameObject.name = "Info";
                        }
                        else meetingInfo = meetingInfoTransform.GetComponent<TextMeshPro>();
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo is not null && playerVoteArea is not null)
                    {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new(0.3384f, 0.0311f, -0.1f);
                    }
                }

                var (tasksCompleted, tasksTotal) = TasksHandler.TaskInfo(p.Data);
                string roleNames = RoleInfoList.GetRolesString(p, true);
                bool WasTaskEnd = tasksCompleted == tasksTotal;

                var completedStr = commsActive ? "?" : tasksCompleted.ToString();
                var color = commsActive ? "808080" : WasTaskEnd ? "00FF00" : "FAD934FF";
                string taskInfo = tasksTotal > 0 ? $"<color=#{color}>({completedStr}/{tasksTotal})</color>" : "";

                string playerInfoText = "";
                string meetingInfoText = "";
                if (p == CachedPlayer.LocalPlayer.PlayerControl)
                {
                    playerInfoText = $"{roleNames} {taskInfo}".Trim();
                    if (FastDestroyableSingleton<HudManager>.Instance.TaskPanel is not null)
                    {
                        TextMeshPro tabText = FastDestroyableSingleton<HudManager>.Instance.TaskPanel.tab.transform.FindChild("TabText_TMP").GetComponent<TextMeshPro>();
                        tabText.SetText($"{FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Tasks)} {taskInfo}");
                    }
                    meetingInfoText = playerInfoText;
                }
                else if (ModMapOptions.GhostsSeeRoles && ModMapOptions.GhostsSeeTasks && !Altruist.Exists)
                {
                    playerInfoText = $"{roleNames} {taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (ModMapOptions.GhostsSeeTasks && !Altruist.Exists)
                {
                    playerInfoText = $"{taskInfo}".Trim();
                    meetingInfoText = playerInfoText;
                }
                else if (ModMapOptions.GhostsSeeRoles && !Altruist.Exists)
                {
                    playerInfoText = $"{roleNames}";
                    meetingInfoText = playerInfoText;
                }

                if (playerInfo is not null)
                {
                    playerInfo.text = playerInfoText;
                    playerInfo.gameObject.SetActive(p.Visible);
                }
                if (meetingInfo is not null) meetingInfo.text = MeetingHud.Instance.state is MeetingHud.VoteStates.Results ? "" : meetingInfoText;
            }
        }
    }

    [HarmonyPatch(nameof(PlayerControl.FixedUpdate)), HarmonyPostfix]
    internal static void Update(PlayerControl __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

        if (PlayerControl.LocalPlayer == __instance)
        {
            UpdateVentButtonVisibility(__instance);

            // Update player outlines
            SetBasePlayerOutlines();
            ImpostorSetTarget();

            // Update Player Info
            UpdatePlayerInfo();
        }

        AmongDogUs.FixedUpdate(__instance);
    }

    internal static bool ShuffleTimingIsKilled = false;

    internal static bool resetToCrewmate = false;
    internal static bool resetToDead = false;

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer)), HarmonyPrefix]
    internal static void MurderPlayerPrefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Allow everyone to murder players
        resetToCrewmate = !__instance.Data.Role.IsImpostor;
        resetToDead = __instance.Data.IsDead;
        __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
        __instance.Data.IsDead = false;
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer)), HarmonyPostfix]
    internal static void MurderPlayerPostfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // Collect dead player info
        DeadPlayer deadPlayer = new(target, DateTime.UtcNow, DeathReason.Kill, __instance);
        deadPlayers.Add(deadPlayer);

        // Reset killer to crewmate if resetToCrewmate
        if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
        if (resetToDead) __instance.Data.IsDead = true;

        // Remove fake tasks when player dies
        if (target.HasFakeTasks()) target.ClearAllTasks();

        // Seer show flash and add dead player position
        foreach (var seer in Seer.AllPlayers)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleType.Seer) && !seer.Data.IsDead && seer != target && Seer.Mode <= 1)
            {
                Helpers.ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
            }
            Seer.DeadBodyPositions?.Add(target.transform.position);
        }

        if (AirShipPatch.IsShuffleElecDoors && ShuffleTimingIsKilled) AirShipPatch.InitializeElecDoor();

        __instance.OnKill(target);
        target.OnDeath(__instance);
    }

    [HarmonyPatch(nameof(PlayerControl.Exiled)), HarmonyPostfix]
    internal static void Exiled(PlayerControl __instance)
    {
        // Collect dead player info
        DeadPlayer deadPlayer = new(__instance, DateTime.UtcNow, DeathReason.Exile, null);
        deadPlayers.Add(deadPlayer);

        // Remove fake tasks when player dies
        if (__instance.HasFakeTasks()) __instance.ClearAllTasks();

        __instance.OnDeath(killer: null);
    }

    [HarmonyPatch(nameof(PlayerControl.SetKillTimer)), HarmonyPrefix]
    internal static bool SetKillTimerPrefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
    {
        if (GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown) <= 0f) return false;
        float multiplier = 1f;
        float addition = 0f;
        if (PlayerControl.LocalPlayer.IsRole(RoleType.BountyHunter)) addition = BountyHunter.AdditionalCooldown;

        float Max = Mathf.Max(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown) * multiplier + addition, __instance.killTimer);
        __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, Max), Max);
        return false;
    }

    internal static void SetKillTimerUnchecked(this PlayerControl player, float time, float Max = float.NegativeInfinity)
    {
        if (Max == float.NegativeInfinity) Max = time;

        player.killTimer = time;
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, Max);
    }
}