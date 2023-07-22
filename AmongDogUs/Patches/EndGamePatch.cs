namespace AmongDogUs.Patches;

internal enum CustomGameOverReason
{
    JesterExiled = 10,
    TeamJackalWin,
    ArsonistWin,

    SabotageReactor,
    SabotageO2,
    ForceEnd,
    EveryoneLose
}

internal enum WinCondition
{
    Default,

    CrewmateWin,
    ImpostorWin,
    JesterWin,
    OpportunistWin,
    JackalWin,
    ArsonistWin,

    ForceEnd,
    EveryoneLose
}

internal static class AdditionalTempData
{
    internal static WinCondition winCondition = WinCondition.Default;
    internal static List<WinCondition> additionalWinConditions = new();
    internal static List<PlayerRoleInfo> playerRoles = new();
    internal static List<byte> WinningPlayers = new();
    internal static GameOverReason gameOverReason;

    internal static void Clear()
    {
        playerRoles.Clear();
        additionalWinConditions.Clear();
        WinningPlayers.Clear();
        winCondition = WinCondition.Default;
    }

    internal class PlayerRoleInfo
    {
        internal string WinOrLose { get; set; }
        internal string PlayerName { get; set; }
        internal List<RoleInfo> Roles { get; set; }
        internal string RoleString { get; set; }
        internal int ColorId = 0;
        internal int TasksCompleted { get; set; }
        internal int TasksTotal { get; set; }
        internal EFinalStatus Status { get; set; }
        internal int PlayerId { get; set; }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal static class OnGameEndPatch
{
    internal static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
        if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
    }

    internal static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        var GameOverReason = AdditionalTempData.gameOverReason;
        AdditionalTempData.Clear();

        var excludeRoles = Array.Empty<RoleType>();
        foreach (var p in GameData.Instance.AllPlayers)
        {
            var roles = RoleInfoList.GetRoleInfoForPlayer(p.Object, excludeRoles, true);
            var (tasksCompleted, tasksTotal) = TasksHandler.TaskInfo(p);
            var finalStatus = finalStatuses[p.PlayerId] =
                p.Disconnected == true ? EFinalStatus.Disconnected :
                finalStatuses.ContainsKey(p.PlayerId) ? finalStatuses[p.PlayerId] :
                p.IsDead == true ? EFinalStatus.Dead :
                GameOverReason == (GameOverReason)CustomGameOverReason.SabotageReactor && !p.Role.IsImpostor ? EFinalStatus.Reactor :
                GameOverReason == (GameOverReason)CustomGameOverReason.SabotageO2 && !p.Role.IsImpostor ? EFinalStatus.LackO2 :
                EFinalStatus.Alive;

            if (GameOverReason == GameOverReason.HumansByTask && p.Object.IsCrew()) tasksCompleted = tasksTotal;

            AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
            {
                PlayerName = p.PlayerName,
                PlayerId = p.PlayerId,
                ColorId = p.DefaultOutfit.ColorId,
                // NameSuffix = Lovers.getIcon(p.Object),
                Roles = roles,
                RoleString = RoleInfoList.GetRolesString(p.Object, true, excludeRoles, true),
                TasksTotal = tasksTotal,
                TasksCompleted = tasksCompleted,
                Status = finalStatus,
            });
        }

        List<PlayerControl> notWinners = new();
        notWinners.AddRange(Jester.AllPlayers);
        notWinners.AddRange(Opportunist.AllPlayers);
        notWinners.AddRange(Madmate.AllPlayers);
        notWinners.AddRange(Jackal.AllPlayers);
        notWinners.AddRange(Sidekick.AllPlayers);
        notWinners.AddRange(Arsonist.AllPlayers);

        List<WinningPlayerData> winnersToRemove = new();
        foreach (WinningPlayerData winner in TempData.winners)
        {
            if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
        }
        foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);


        bool JesterWin = Jester.Exists && GameOverReason is (GameOverReason)CustomGameOverReason.JesterExiled;
        bool TeamJackalWin = GameOverReason is (GameOverReason)CustomGameOverReason.TeamJackalWin && (Jackal.LivingPlayers.Count > 0 || (Sidekick.AllPlayers.Count > 0 && Sidekick.LivingPlayers.Count >= 0) || Sidekick.AllPlayers.Count <= 0);
        bool ArsonistWin = Arsonist.Exists && GameOverReason is (GameOverReason)CustomGameOverReason.ArsonistWin;

        bool CrewmateWin = GameOverReason is GameOverReason.HumansByTask or GameOverReason.HumansByVote;
        bool ImpostorWin = GameOverReason is GameOverReason.ImpostorByKill or GameOverReason.ImpostorBySabotage or GameOverReason.ImpostorByVote;
        bool ForceEnd = GameOverReason is (GameOverReason)CustomGameOverReason.ForceEnd;
        bool SaboWin = GameOverReason is GameOverReason.ImpostorBySabotage;
        bool EveryoneLose = AdditionalTempData.playerRoles.All(x => x.Status != EFinalStatus.Alive);

        if (JesterWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var jester in Jester.players)
            {
                WinningPlayerData wpd = new(jester.player.Data);
                TempData.winners.Add(wpd);
                jester.player.Data.IsDead = true;
                AdditionalTempData.WinningPlayers.Add(jester.player.Data.PlayerId);
            }
            AdditionalTempData.winCondition = WinCondition.JesterWin;
        }

        else if (TeamJackalWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var jackal in Jackal.players)
            {
                WinningPlayerData wpd = new(jackal.player.Data)
                {
                    IsImpostor = false
                };
                TempData.winners.Add(wpd);
                AdditionalTempData.WinningPlayers.Add(jackal.player.Data.PlayerId);
            }

            foreach (var sidekick in Sidekick.players)
            {
                WinningPlayerData wpd = new(sidekick.player.Data)
                {
                    IsImpostor = false
                };
                TempData.winners.Add(wpd);
                AdditionalTempData.WinningPlayers.Add(sidekick.player.Data.PlayerId);
            }
            AdditionalTempData.winCondition = WinCondition.JackalWin;
        }

        else if (ArsonistWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var arsonist in Arsonist.players)
            {
                WinningPlayerData wpd = new(arsonist.player.Data);
                TempData.winners.Add(wpd);
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
                AdditionalTempData.WinningPlayers.Add(arsonist.player.Data.PlayerId);
            }
        }

        else if (CrewmateWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsCrew())
                {
                    WinningPlayerData wpd = new(player.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.WinningPlayers.Add(player.Data.PlayerId);
                }
            }
            AdditionalTempData.winCondition = WinCondition.CrewmateWin;
        }

        else if (ImpostorWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsImpostor())
                {
                    WinningPlayerData wpd = new(player.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.WinningPlayers.Add(player.Data.PlayerId);
                }
            }
            AdditionalTempData.winCondition = WinCondition.ImpostorWin;
        }

        else if (ForceEnd)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player != null)
                {
                    WinningPlayerData wpd = new(player.Data);
                    TempData.winners.Add(wpd);
                    player.Data.IsDead = false;
                }
            }
            AdditionalTempData.winCondition = WinCondition.ForceEnd;
        }

        else if (EveryoneLose)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            AdditionalTempData.winCondition = WinCondition.EveryoneLose;
        }

        if (!SaboWin)
        {
            bool oppWin = false;
            foreach (var p in Opportunist.AllPlayers)
            {
                if (!TempData.winners.ToArray().Any(x => x.PlayerName == p.Data.PlayerName) && !p.Data.IsDead) TempData.winners.Add(new WinningPlayerData(p.Data));
                oppWin = true;
            }
            if (oppWin) AdditionalTempData.additionalWinConditions.Add(WinCondition.OpportunistWin);
        }

        if (Madmate.Exists && ImpostorWin)
        {
            if (!Madmate.HasTasks || (Madmate.HasTasks && Madmate.CanWinTaskEnd && Madmate.TasksComplete(PlayerControl.LocalPlayer)))
            {
                foreach (var p in Madmate.AllPlayers)
                {
                    WinningPlayerData wpd = new(p.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.WinningPlayers.Add(p.Data.PlayerId);
                }
            }
        }

        foreach (WinningPlayerData wpd in TempData.winners)
        {
            wpd.IsDead = wpd.IsDead || AdditionalTempData.playerRoles.Any(x => x.PlayerName == wpd.PlayerName && x.Status != EFinalStatus.Alive);
        }

        // Reset Settings
        RPCProcedure.ResetVariables();
    }

    internal class EndGameNavigationPatch
    {
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        internal class EndGameManagerSetUpPatch
        {
            internal static void Postfix(EndGameManager __instance)
            {
                // Delete and readd PoolablePlayers always showing the name and role of the player
                foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
                {
                    Object.Destroy(pb.gameObject);
                }
                int Num = Mathf.CeilToInt(7.5f);
                List<WinningPlayerData> list = TempData.winners.ToArray().ToList().OrderBy(delegate (WinningPlayerData b)
                {
                    if (!b.IsYou)
                    {
                        return 0;
                    }
                    return -1;
                }).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    WinningPlayerData winningPlayerData2 = list[i];
                    int Num2 = (i % 2 == 0) ? -1 : 1;
                    int Num3 = (i + 1) / 2;
                    float num3 = Num3;
                    float Num4 = num3 / Num;
                    float Num5 = Mathf.Lerp(1f, 0.75f, Num4);
                    float Num6 = (i == 0) ? -8 : -1;
                    PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                    poolablePlayer.transform.localPosition = new Vector3(1f * Num2 * Num3 * Num5, FloatRange.SpreadToEdges(-1.125f, 0f, Num3, Num), Num6 + (float)Num3 * 0.01f) * 0.9f;
                    float Num7 = Mathf.Lerp(1f, 0.65f, Num4) * 0.9f;
                    Vector3 vector = new(Num7, Num7, 1f);
                    poolablePlayer.transform.localScale = vector;
                    poolablePlayer.UpdateFromPlayerOutfit((GameData.PlayerOutfit)winningPlayerData2, PlayerMaterial.MaskType.ComplexUI, winningPlayerData2.IsDead, true);
                    if (winningPlayerData2.IsDead)
                    {
                        poolablePlayer.cosmetics.currentBodySprite.BodySprite.sprite = poolablePlayer.cosmetics.currentBodySprite.GhostSprite;
                        poolablePlayer.SetDeadFlipX(i % 2 == 0);
                    }
                    else
                    {
                        poolablePlayer.SetFlipX(i % 2 == 0);
                    }

                    poolablePlayer.cosmetics.nameText.color = Color.white;
                    poolablePlayer.cosmetics.nameText.lineSpacing *= 0.7f;
                    poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                    poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                    poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

                    foreach (var data in AdditionalTempData.playerRoles)
                    {
                        if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                        poolablePlayer.cosmetics.nameText.text += $"\n<size=80%>{data.RoleString}</size>";
                    }
                }

                // Additional code
                GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
                bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                TMP_Text textRenderer = bonusTextObject.GetComponent<TMP_Text>();
                textRenderer.text = "";

                string bonusText = "";

                if (AdditionalTempData.winCondition == WinCondition.JesterWin)
                {
                    bonusText = "JesterWin";
                    textRenderer.color = JesterPink;
                    __instance.BackgroundBar.material.SetColor("_Color", JesterPink);
                    // if (ModMapOptions.EnableCustomSounds)
                    // {
                    //     SoundManager.Instance.StopSound(__instance.ImpostorStinger);
                    //     SoundManager.Instance.PlaySound(JesterWinSound, false, 0.8f);
                    // }
                }
                else if (AdditionalTempData.winCondition == WinCondition.JackalWin)
                {
                    bonusText = "TeamJackalWin";
                    textRenderer.color = JackalBlue;
                    __instance.BackgroundBar.material.SetColor("_Color", JackalBlue);
                }
                else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
                {
                    bonusText = "ArsonistWin";
                    textRenderer.color = ArsonistOrange;
                    __instance.BackgroundBar.material.SetColor("_Color", ArsonistOrange);
                }
                else if (AdditionalTempData.gameOverReason is GameOverReason.HumansByTask or GameOverReason.HumansByVote)
                {
                    bonusText = "CrewmateWin";
                    textRenderer.color = CrewmateBlue;
                }
                else if (AdditionalTempData.gameOverReason is GameOverReason.ImpostorByKill or GameOverReason.ImpostorByVote or GameOverReason.ImpostorBySabotage)
                {
                    bonusText = "ImpostorWin";
                    textRenderer.color = ImpostorRed;
                }
                else if (AdditionalTempData.winCondition == WinCondition.EveryoneLose)
                {
                    __instance.WinText.text = "EveryoneLose";
                    textRenderer.color = DisabledGrey;
                    __instance.BackgroundBar.material.SetColor("_Color", DisabledGrey);
                    // if (ModMapOptions.EnableCustomSounds)
                    // {
                    //     SoundManager.Instance.StopSound(__instance.ImpostorStinger);
                    //     SoundManager.Instance.PlaySound(EveryoneLoseSound, false, 0.8f);
                    // }
                }
                else if (AdditionalTempData.winCondition == WinCondition.ForceEnd)
                {
                    bonusText = "ForceEnd";
                    textRenderer.color = DisabledGrey;
                    __instance.BackgroundBar.material.SetColor("_Color", DisabledGrey);
                    SoundManager.Instance.StopSound(__instance.ImpostorStinger);
                    SoundManager.Instance.PlaySound(__instance.DisconnectStinger, false, 0.8f);
                }

                string extraText = "";
                foreach (WinCondition w in AdditionalTempData.additionalWinConditions)
                {
                    switch (w)
                    {
                        case WinCondition.OpportunistWin:
                            extraText += ModResources.OpportunistExtra;
                            break;
                        default:
                            break;
                    }
                }

                if (extraText.Length > 0 && AdditionalTempData.winCondition != WinCondition.ForceEnd)
                {
                    textRenderer.text = bonusText + string.Format(AmongDogUs.GetString($"{bonusText}Extra"), extraText);
                }
                else
                {
                    textRenderer.text = AmongDogUs.GetString(bonusText);
                }

                if (AdditionalTempData.gameOverReason == (GameOverReason)CustomGameOverReason.SabotageO2)
                {
                    textRenderer.text += $"\n{ModResources.O2Win}";
                }
                else if (AdditionalTempData.gameOverReason == (GameOverReason)CustomGameOverReason.SabotageReactor)
                {
                    textRenderer.text += $"\n{ModResources.ReactorWin}";
                }
                else if (AdditionalTempData.gameOverReason == GameOverReason.HumansByTask)
                {
                    textRenderer.text += $"\n{ModResources.TaskWin}";
                }
                else if (AdditionalTempData.gameOverReason == (GameOverReason)CustomGameOverReason.ForceEnd)
                {
                    textRenderer.text += $"\n{ModResources.FinishedByHost}";
                }

                if (ModMapOptions.ShowRoleSummary)
                {
                    var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                    GameObject roleSummary = Object.Instantiate(__instance.WinText.gameObject);
                    roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
                    roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                    var RoleSummaryText = new StringBuilder();
                    RoleSummaryText.AppendLine(ModResources.RoleSummaryText);
                    AdditionalTempData.playerRoles.Sort((x, y) =>
                    {
                        RoleInfo roleX = x.Roles.FirstOrDefault();
                        RoleInfo roleY = y.Roles.FirstOrDefault();
                        RoleType idX = roleX == null ? RoleType.NoRole : roleX.RoleId;
                        RoleType idY = roleY == null ? RoleType.NoRole : roleY.RoleId;

                        if (x.Status == y.Status)
                        {
                            if (idX == idY)
                            {
                                return x.PlayerName.CompareTo(y.PlayerName);
                            }
                            return idX.CompareTo(idY);
                        }
                        return x.Status.CompareTo(y.Status);
                    });

                    foreach (var data in AdditionalTempData.playerRoles)
                    {
                        var TaskInfo = data.TasksTotal > 0 ? $"<color=#FAD934FF>{data.TasksCompleted}/{data.TasksTotal}</color>" : "";
                        string AliveDead = AmongDogUs.GetString($"RoleSummary{data.Status}");
                        string result = $"{data.PlayerName/* + data.NameSuffix*/}<pos=18.5%>{TaskInfo}<pos=25%>{AliveDead}<pos=34%>{data.RoleString}";

                        RoleSummaryText.AppendLine(result);
                    }

                    TMP_Text RoleSummaryTextMesh = roleSummary.GetComponent<TMP_Text>();
                    RoleSummaryTextMesh.alignment = TextAlignmentOptions.TopLeft;
                    RoleSummaryTextMesh.color = Color.white;
                    RoleSummaryTextMesh.outlineWidth *= 1.2f;
                    RoleSummaryTextMesh.fontSizeMin = 1.25f;
                    RoleSummaryTextMesh.fontSizeMax = 1.25f;
                    RoleSummaryTextMesh.fontSize = 1.25f;

                    var RoleSummaryTextMeshRectTransform = RoleSummaryTextMesh.GetComponent<RectTransform>();
                    RoleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                    RoleSummaryTextMesh.text = RoleSummaryText.ToString();
                }
                AdditionalTempData.Clear();
            }

            [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
            internal class CheckEndCriteriaPatch
            {
                internal static bool Prefix()
                {
                    if (!GameData.Instance) return false;
                    if (DestroyableSingleton<TutorialManager>.InstanceExists) return true; // InstanceExists | Don't check Custom Criteria when in Tutorial
                    if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed) return false;

                    ShipStatus __instance = ShipStatus.Instance;
                    var statistics = new PlayerStatistics();
                    if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForSabotageWin(__instance)) return false;
                    if (CheckAndEndGameForTaskWin(__instance)) return false;
                    if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
                    return false;
                }

                private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive &&
                        statistics.TeamImpostorsAlive == 0)
                    {
                        UncheckedEndGame(CustomGameOverReason.TeamJackalWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
                {
                    if (__instance.Systems == null) return false;
                    ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
                    if (systemType != null)
                    {
                        LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                        if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                        {
                            EndGameForO2(__instance);
                            lifeSuppSystemType.Countdown = 10000f;
                            return true;
                        }
                    }
                    ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
                    systemType2 ??= __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
                    if (systemType2 != null)
                    {
                        ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                        if (criticalSystem != null && criticalSystem.Countdown < 0f)
                        {
                            EndGameForReactor(__instance);
                            criticalSystem.ClearSabotage();
                            return true;
                        }
                    }
                    return false;
                }

                private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
                {
                    if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
                    {
                        UncheckedEndGame(GameOverReason.HumansByTask);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive &&
                        statistics.TeamJackalAlive == 0)
                    {
                        var endReason = TempData.LastDeathReason switch
                        {
                            DeathReason.Exile => GameOverReason.ImpostorByVote,
                            DeathReason.Kill => GameOverReason.ImpostorByKill,
                            _ => GameOverReason.ImpostorByVote,
                        };
                        UncheckedEndGame(endReason);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamCrew > 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
                    {
                        UncheckedEndGame(GameOverReason.HumansByVote);
                        return true;
                    }
                    return false;
                }

                private static void EndGameForO2(ShipStatus __instance)
                {
                    UncheckedEndGame(CustomGameOverReason.SabotageO2);
                    return;
                }

                private static void EndGameForReactor(ShipStatus __instance)
                {
                    UncheckedEndGame(CustomGameOverReason.SabotageReactor);
                    return;
                }

                private static void UncheckedEndGame(GameOverReason reason)
                {
                    GameManager.Instance.RpcEndGame(reason, false);
                }

                internal static void UncheckedEndGame(CustomGameOverReason reason)
                {
                    UncheckedEndGame((GameOverReason)reason);
                }
            }

            internal class PlayerStatistics
            {
                internal int TeamImpostorsAlive { get; set; }
                internal int TeamCrew { get; set; }
                internal int TeamJackalAlive { get; set; }
                internal int NeutralAlive { get; set; }
                internal int TotalAlive { get; set; }

                internal PlayerStatistics()
                {
                    GetPlayerCounts();
                }

                private void GetPlayerCounts()
                {
                    int NumJackalAlive = 0;
                    int NumImpostorsAlive = 0;
                    int NumTotalAlive = 0;
                    int NumNeutralAlive = 0;
                    int NumCrew = 0;

                    foreach (var playerInfo in GameData.Instance.AllPlayers)
                    {
                        if (!playerInfo.Disconnected)
                        {
                            if (playerInfo.Object.IsCrew()) NumCrew++;
                            if (!playerInfo.IsDead)
                            {
                                NumTotalAlive++;
                                if (playerInfo.Role.IsImpostor) NumImpostorsAlive++;
                                if (playerInfo.Object.IsNeutral()) NumNeutralAlive++;
                                if (playerInfo.Object.IsTeamJackal()) NumJackalAlive++;
                            }
                        }
                    }

                    TeamCrew = NumCrew;
                    TeamImpostorsAlive = NumImpostorsAlive;
                    TeamJackalAlive = NumJackalAlive;
                    NeutralAlive = NumNeutralAlive;
                    TotalAlive = NumTotalAlive;
                }
            }
        }
    }
}