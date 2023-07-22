namespace AmongDogUs;

// Submerged Custom RPC
//
// 9 - SubmarineStatus
//
// 210 - SetCustomData
// 211 - RequestChangeFloor
// 212 - AcknowledgeChangeFloor
// 213 - EngineVent
// 214 - OxygenDeath
//
// 130 - SubmarineOxygenSystem
// 136 - SubmarineElevatorSystem (WestLeft)
// 137 - SubmarineElevatorSystem (WestRight)
// 138 - SubmarineElevatorSystem (EastLeft)
// 139 - SubmarineElevatorSystem (EastRight)
// 140 - SubmarineElevatorSystem (Service)
// 141 - SubmarinePlayerFloorSystem
// 142 - SubmarineSecuritySabotageSystem
// 143 - SubmarineSpawnInSystem
// 144 - SubmarineBoxCatSystem

internal enum ECustomRPC : byte
{
    ResetVariables = 60,
    ShareOptions,
    DynamicMapOption,
    VersionHandshake,
    SetRole,
    AddModifier,
    UseAdminTime,
    UseCameraTime,
    UseVitalsTime,
    UncheckedMurderPlayer,
    SheriffKill = 70,
    EngineerFixLights,
    EngineerUsedRepair,
    UncheckedSetTasks,
    UncheckedEndGame,
    DragPlaceBody,
    CleanBody,
    BakeryBomb,
    TeleporterTeleport,
    JackalCreatesSidekick,
    SidekickPromotes = 80,
    ArsonistDouse,
    ArsonistWin,
    AltruistKill,
    AltruistRevive,
    UncheckedCmdReportDeadBody,
    EngineerFixSubmergedOxygen,
}

internal static class RPCProcedure
{
    [HarmonyPatch(typeof(PlayerControl))]
    internal class RPCHandlerPatch
    {
        [HarmonyPatch(nameof(PlayerControl.HandleRpc)), HarmonyPostfix]
        internal static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            byte packetId = callId;
            switch (packetId)
            {
                // 60
                case (byte)ECustomRPC.ResetVariables:
                    ResetVariables();
                    break;
                // 61
                case (byte)ECustomRPC.ShareOptions:
                    ShareOptions((int)reader.ReadPackedUInt32(), reader);
                    break;
                // 62
                case (byte)ECustomRPC.DynamicMapOption:
                    byte mapId = reader.ReadByte();
                    DynamicMapOption(mapId);
                    break;
                // 63
                case (byte)ECustomRPC.VersionHandshake:
                    int major = reader.ReadPackedInt32();
                    int minor = reader.ReadPackedInt32();
                    int patch = reader.ReadPackedInt32();
                    int versionOwnerId = reader.ReadPackedInt32();
                    byte revision = 0xFF;
                    Guid guid;
                    if (reader.Length - reader.Position >= 17)
                    {
                        // enough bytes left to read
                        revision = reader.ReadByte();
                        // GUID
                        byte[] GBytes = reader.ReadBytes(16);
                        guid = new(GBytes);
                    }
                    else guid = new(new byte[16]);
                    VersionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                    break;
                // 64
                case (byte)ECustomRPC.SetRole:
                    byte roleId = reader.ReadByte();
                    byte playerId = reader.ReadByte();
                    SetRole(roleId, playerId);
                    break;
                // 65
                case (byte)ECustomRPC.AddModifier:
                    AddModifier(reader.ReadByte(), reader.ReadByte());
                    break;
                // 66
                case (byte)ECustomRPC.UseAdminTime:
                    UseAdminTime(reader.ReadSingle());
                    break;
                // 67
                case (byte)ECustomRPC.UseCameraTime:
                    UseCameraTime(reader.ReadSingle());
                    break;
                // 68
                case (byte)ECustomRPC.UseVitalsTime:
                    UseVitalsTime(reader.ReadSingle());
                    break;
                // 69
                case (byte)ECustomRPC.UncheckedMurderPlayer:
                    byte source = reader.ReadByte();
                    byte target = reader.ReadByte();
                    byte showAnimation = reader.ReadByte();
                    UncheckedMurderPlayer(source, target, showAnimation);
                    break;
                // 70
                case (byte)ECustomRPC.SheriffKill:
                    SheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                    break;
                // 71
                case (byte)ECustomRPC.EngineerFixLights:
                    EngineerFixLights();
                    break;
                // 72
                case (byte)ECustomRPC.EngineerUsedRepair:
                    EngineerUsedRepair(reader.ReadByte());
                    break;
                // 73
                case (byte)ECustomRPC.UncheckedSetTasks:
                    UncheckedSetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
                    break;
                // 74
                case (byte)ECustomRPC.UncheckedEndGame:
                    UncheckedEndGame(reader.ReadByte());
                    break;
                // 75
                case (byte)ECustomRPC.DragPlaceBody:
                    DragPlaceBody(reader.ReadByte());
                    break;
                // 76
                case (byte)ECustomRPC.CleanBody:
                    CleanBody(reader.ReadByte());
                    break;
                // 77
                case (byte)ECustomRPC.BakeryBomb:
                    BakeryBomb(reader.ReadByte());
                    break;
                // 78
                case (byte)ECustomRPC.TeleporterTeleport:
                    TeleporterTeleport(reader.ReadByte());
                    break;
                // 79
                case (byte)ECustomRPC.JackalCreatesSidekick:
                    JackalCreatesSidekick(reader.ReadByte());
                    break;
                // 80
                case (byte)ECustomRPC.SidekickPromotes:
                    SidekickPromotes(reader.ReadByte());
                    break;
                // 81
                case (byte)ECustomRPC.ArsonistDouse:
                    ArsonistDouse(reader.ReadByte());
                    break;
                // 82
                case (byte)ECustomRPC.ArsonistWin:
                    ArsonistWin();
                    break;
                // 83
                case (byte)ECustomRPC.AltruistKill:
                    AltruistKill(reader.ReadByte());
                    break;
                // 84
                case (byte)ECustomRPC.AltruistRevive:
                    byte parentId = reader.ReadByte();
                    byte AltruistId = reader.ReadByte();
                    AltruistRevive(parentId, AltruistId);
                    break;
                // 85
                case (byte)ECustomRPC.UncheckedCmdReportDeadBody:
                    byte reportSource = reader.ReadByte();
                    byte reportTarget = reader.ReadByte();
                    UncheckedCmdReportDeadBody(reportSource, reportTarget);
                    break;
                case (byte)ECustomRPC.EngineerFixSubmergedOxygen:
                    EngineerFixSubmergedOxygen();
                    break;
            }
        }
    }

    internal static void ResetVariables()
    {
        ModMapOptions.ClearAndReloadOptions();
        AmongDogUs.ClearAndReloadRoles();
        ClearGameHistory();
        AdminPatch.ResetData();
        CameraPatch.ResetData();
        VitalsPatch.ResetData();
        RoleData.SetCustomButtonCooldowns();
        CustomOverlays.ResetOverlays();
        MapBehaviorPatch.ResetIcons();
    }

    internal static void ShareOptions(int NumberOfOptions, MessageReader reader)
    {
        try
        {
            for (int i = 0; i < NumberOfOptions; i++)
            {
                uint optionId = reader.ReadPackedUInt32();
                uint selection = reader.ReadPackedUInt32();
                CustomOption option = CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                option.UpdateSelection((int)selection);
            }
        }
        catch (Exception e)
        {
            Main.Logger.LogError("Error while deserializing options: " + e.Message);
        }
    }

    internal static void DynamicMapOption(byte mapId)
    {
        GameManager.Instance.LogicOptions.currentGameOptions.SetByte(ByteOptionNames.MapId, mapId);
    }

    internal static void VersionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
    {
        Version ver;
        if (revision < 0) ver = new Version(major, minor, build);
        else ver = new Version(major, minor, build, revision);
        GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
    }

    internal static void SetRole(byte roleId, byte playerId)
    {
        PlayerControl.AllPlayerControls.ToArray().DoIf(
            x => x.PlayerId == playerId,
            x => x.SetRole((RoleType)roleId)
        );
    }

    internal static void AddModifier(byte modId, byte playerId)
    {
        PlayerControl.AllPlayerControls.ToArray().DoIf(
            x => x.PlayerId == playerId,
            x => x.AddModifier((ModifierType)modId)
        );
    }

    internal static void UseAdminTime(float time)
    {
        ModMapOptions.RestrictAdminTime -= time;
    }

    internal static void UseCameraTime(float time)
    {
        ModMapOptions.RestrictCamerasTime -= time;
    }

    internal static void UseVitalsTime(float time)
    {
        ModMapOptions.RestrictVitalsTime -= time;
    }

    internal static void UncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
    {
        PlayerControl source = Helpers.PlayerById(sourceId);
        PlayerControl target = Helpers.PlayerById(targetId);
        if (source != null && target != null)
        {
            if (showAnimation == 0) KillAnimationPatch.hideNextAnimation = true;
            source.MurderPlayer(target);
        }
    }

    internal static void SheriffKill(byte sheriffId, byte targetId, bool misfire)
    {
        PlayerControl sheriff = Helpers.PlayerById(sheriffId);
        PlayerControl target = Helpers.PlayerById(targetId);
        if (sheriff == null || target == null) return;

        if (sheriff != null) Sheriff.ReamingShots--;

        if (misfire)
        {
            sheriff.MurderPlayer(sheriff);
            finalStatuses[sheriffId] = EFinalStatus.Misfire;

            if (!Sheriff.MisfireKillsTarget) return;
            finalStatuses[targetId] = EFinalStatus.Misfire;
        }

        sheriff.MurderPlayer(target);
    }

    internal static void UpdateMeeting(byte targetId, bool dead = true)
    {
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == targetId)
                {
                    pva.SetDead(pva.DidReport, dead);
                    pva.Overlay.gameObject.SetActive(dead);
                }

                // Give players back their vote if target is shot dead
                if (Helpers.RefundVotes && dead)
                {
                    if (pva.VotedFor != targetId) continue;
                    pva.UnsetVote();
                    var voteAreaPlayer = Helpers.PlayerById(pva.TargetPlayerId);
                    if (!voteAreaPlayer.AmOwner) continue;
                    MeetingHud.Instance.ClearVote();
                }
            }

            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
    }

    internal static void EngineerFixLights()
    {
        SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
    }

    internal static void EngineerUsedRepair(byte engineerId)
    {
        PlayerControl engineer = Helpers.PlayerById(engineerId);
        if (engineer != null) ProEngineer.ReamingCounts--;
    }

    internal static void UncheckedSetTasks(byte playerId, byte[] taskTypeIds)
    {
        var player = Helpers.PlayerById(playerId);
        player.ClearAllTasks();

        GameData.Instance.SetTasks(playerId, taskTypeIds);
    }

    internal static void UncheckedEndGame(byte GameOverReason)
    {
        OnGameEndPatch.EndGameNavigationPatch.EndGameManagerSetUpPatch.CheckEndCriteriaPatch.UncheckedEndGame((CustomGameOverReason)GameOverReason);
    }

    internal static void DragPlaceBody(byte playerId)
    {
        DeadBody[] Array = Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < Array.Length; i++)
        {
            if (GameData.Instance.GetPlayerById(Array[i].ParentId).PlayerId == playerId)
            {
                if (!UnderTaker.DraggingBody)
                {
                    UnderTaker.DraggingBody = true;
                    UnderTaker.BodyId = playerId;
                    if (GameManager.Instance.LogicOptions.currentGameOptions.GetByte(ByteOptionNames.MapId) == 5)
                    {
                        GameObject vent = GameObject.Find("LowerCentralVent");
                        vent.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
                else
                {
                    UnderTaker.DraggingBody = false;
                    UnderTaker.BodyId = 0;
                    foreach (var underTaker in UnderTaker.AllPlayers)
                    {
                        var currentPosition = underTaker.GetTruePosition();
                        var velocity = underTaker.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
                        var newPos = underTaker.GetTruePosition() - (velocity / 3) + new Vector2(0.15f, 0.25f) + Array[i].myCollider.offset;
                        if (!PhysicsHelpers.AnythingBetween(
                            currentPosition,
                            newPos,
                            Constants.ShipAndObjectsMask,
                            false
                        ))
                        {
                            if (GameManager.Instance.LogicOptions.currentGameOptions.GetByte(ByteOptionNames.MapId) == 5)
                            {
                                Array[i].transform.position = newPos;
                                Array[i].transform.position += new Vector3(0, 0, -0.5f);
                                GameObject vent = GameObject.Find("LowerCentralVent");
                                vent.GetComponent<BoxCollider2D>().enabled = true;
                            }
                            else
                            {
                                Array[i].transform.position = newPos;
                            }
                        }
                    }
                }
            }
        }
    }

    internal static void UnderTakerReSetValues()
    {
        // Restore UnderTaker values when rewind time
        if (PlayerControl.LocalPlayer.IsRole(RoleType.UnderTaker) && UnderTaker.DraggingBody)
        {
            UnderTaker.DraggingBody = false;
            UnderTaker.BodyId = 0;
        }
    }

    internal static void CleanBody(byte playerId)
    {
        DeadBody[] Array = Object.FindObjectsOfType<DeadBody>();
        for (int i = 0; i < Array.Length; i++)
        {
            if (GameData.Instance.GetPlayerById(Array[i].ParentId).PlayerId == playerId)
            {
                Object.Destroy(Array[i].gameObject);
            }
        }
    }

    internal static void BakeryBomb(byte BakeryId)
    {
        var bakery = Helpers.PlayerById(BakeryId);

        bakery.Exiled();
        finalStatuses[BakeryId] = EFinalStatus.Bomb;
        Main.Logger.LogInfo("Bakery exploded!");
    }

    internal static void TeleporterTeleport(byte playerId)
    {
        var p = Helpers.PlayerById(playerId);
        PlayerControl.LocalPlayer.transform.position = p.transform.position;
        _ = new CustomMessage(string.Format(ModResources.TeleporterTeleported, p.cosmetics.nameText.text), 3);
        // SoundManager.Instance.PlaySound(Teleport, false, 0.8f);
    }

    internal static void ErasePlayerRoles(byte playerId, bool ClearNeutralTasks = true)
    {
        PlayerControl player = Helpers.PlayerById(playerId);
        if (player == null) return;

        // Don't give a former neutral role tasks because that destroys the balance.
        if (player.IsNeutral() && ClearNeutralTasks) player.ClearAllTasks();

        player.EraseAllRoles();
        player.EraseAllModifiers();
    }

    internal static void JackalCreatesSidekick(byte targetId)
    {
        PlayerControl Player = Helpers.PlayerById(targetId);
        if (Player == null) return;

        FastDestroyableSingleton<RoleManager>.Instance.SetRole(Player, RoleTypes.Crewmate);
        ErasePlayerRoles(Player.PlayerId, true);
        Player.SetRole(RoleType.Sidekick);
        if (Player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;

        Jackal.CanSidekick = false;
    }

    internal static void SidekickPromotes(byte sidekickId)
    {
        PlayerControl sidekick = Helpers.PlayerById(sidekickId);
        ErasePlayerRoles(sidekickId);
        sidekick.SetRole(RoleType.Jackal);
        Jackal.CanSidekick = true;
    }

    internal static void ArsonistDouse(byte playerId)
    {
        Arsonist.DousedPlayers.Add(Helpers.PlayerById(playerId));
    }

    internal static void ArsonistWin()
    {
        UncheckedEndGame((byte)CustomGameOverReason.ArsonistWin);
        var livingPlayers = PlayerControl.AllPlayerControls.ToArray().Where(p => !p.IsRole(RoleType.Arsonist) && p.IsAlive());
        foreach (PlayerControl p in livingPlayers)
        {
            p.Exiled();
            finalStatuses[p.PlayerId] = EFinalStatus.Torched;
        }
    }

    internal static void AltruistKill(byte AltruistId)
    {
        UncheckedMurderPlayer(AltruistId, AltruistId, 0);
        finalStatuses[AltruistId] = EFinalStatus.Suicide;
    }

    internal static void AltruistRevive(byte parentId, byte AltruistId)
    {
        PlayerControl Altruist = Helpers.PlayerById(AltruistId);
        PlayerControl TargetPlayer = Helpers.PlayerById(parentId);
        DeadBody Target = Helpers.DeadBodyById(parentId);

        if (Altruist || Target || TargetPlayer == null) return;

        var Position = Target.TruePosition;
        CleanBody(parentId);

        foreach (DeadBody deadBody in Object.FindObjectsOfType<DeadBody>()) if (deadBody.ParentId == AltruistId) CleanBody(AltruistId);
        TargetPlayer.Revive();
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(TargetPlayer, TargetPlayer.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate);
        finalStatuses[TargetPlayer.PlayerId] = EFinalStatus.Revival;
        TargetPlayer.NetTransform.SnapTo(new(Position.x, Position.y + 0.3636f));
    }

    internal static void UncheckedCmdReportDeadBody(byte sourceId, byte targetId)
    {
        PlayerControl source = Helpers.PlayerById(sourceId);
        var t = targetId == byte.MaxValue ? null : Helpers.PlayerById(targetId).Data;
        source?.ReportDeadBody(t);
    }

    internal static void EngineerFixSubmergedOxygen()
    {
        SubmergedCompatibility.RepairOxygen();
    }
}