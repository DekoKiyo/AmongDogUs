namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(HudManager))]
internal static class HudManagerPatch
{
    [HarmonyPatch(nameof(HudManager.Start)), HarmonyPostfix]
    internal static void SetupButton(HudManager __instance)
    {
        RoleData.MakeButtons(__instance);
        RoleData.SetCustomButtonCooldowns();
    }

    [HarmonyPatch(nameof(HudManager.OpenMeetingRoom)), HarmonyPrefix]
    internal static void OpenMeetingRoom()
    {
        AmongDogUs.OnMeetingStart();
    }

    [HarmonyPatch(nameof(HudManager.Update)), HarmonyPostfix]
    internal static void Update()
    {
        if (AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Started)
        {
            CustomButton.HudUpdate();

            if (AmongUsClient.Instance.AmHost && Input.GetKeyDown(KeyCode.F11))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UncheckedEndGame, SendOption.Reliable, -1);
                writer.Write((byte)CustomGameOverReason.ForceEnd);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.UncheckedEndGame((byte)CustomGameOverReason.ForceEnd);
            }

            ResetNameTagsAndColors();
            SetNameColors();
        }

        var GameLoadAnimation = GameObject.Find("FullScreen500(Clone)/GameLoadAnimation");
        if (GameLoadAnimation is not null && GameLoadAnimation.active)
        {
            GameLoadAnimation.SetActive(false);
            Main.Logger.LogWarning("[WARNING] The Crew that Loading Animation was disabled with mod!");
        }
        var GameLoadAnimation2 = GameObject.Find("FullScreen500(Clone)(Clone)/GameLoadAnimation");
        if (GameLoadAnimation2 is not null && GameLoadAnimation2.active)
        {
            GameLoadAnimation2.SetActive(false);
            Main.Logger.LogWarning("[WARNING] The Crew that Loading Animation was disabled with mod!");
        }
    }

    // [HarmonyPatch(nameof(HudManager.SetHudActive)), HarmonyPostfix]
    // internal static void SetHudActive(HudManager __instance)
    // {
    //     FastDestroyableSingleton<HudManager>.Instance.transform.FindChild("TaskDisplay").FindChild("TaskPanel").gameObject.SetActive(true);
    // }

    internal static void ResetNameTagsAndColors() { }

    internal static void SetPlayerNameColor(PlayerControl p, Color color)
    {
        p.cosmetics.nameText.color = color;
        if (MeetingHud.Instance != null) foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                if (player.NameText != null && p.PlayerId == player.TargetPlayerId) player.NameText.color = color;
    }

    internal static void SetNameColors()
    {
        var p = PlayerControl.LocalPlayer;

        switch (p.GetRoleId())
        {
            case RoleType.Impostor:
            case RoleType.CustomImpostor:
            case RoleType.UnderTaker:
            case RoleType.BountyHunter:
            case RoleType.Teleporter:
            case RoleType.EvilHacker:
                SetPlayerNameColor(p, ImpostorRed);
                break;

            case RoleType.Sheriff: SetPlayerNameColor(p, SheriffYellow); break;
            case RoleType.ProEngineer: SetPlayerNameColor(p, EngineerBlue); break;
            case RoleType.Bakery: SetPlayerNameColor(p, BakeryYellow); break;
            case RoleType.Snitch: SetPlayerNameColor(p, SnitchGreen); break;
            case RoleType.Seer: SetPlayerNameColor(p, SeerGreen); break;
            case RoleType.Lighter: SetPlayerNameColor(p, LighterYellow); break;
            case RoleType.Altruist: SetPlayerNameColor(p, AltruistRed); break;
            case RoleType.Mayor: SetPlayerNameColor(p, MayorGreen); break;
            case RoleType.Crewmate: SetPlayerNameColor(p, CrewmateBlue); break;

            case RoleType.Jester: SetPlayerNameColor(p, JesterPink); break;
            case RoleType.Arsonist: SetPlayerNameColor(p, ArsonistOrange); break;

            case RoleType.Madmate:
                SetPlayerNameColor(p, ImpostorRed);
                if (Madmate.KnowsImpostors(p))
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        if (pc.IsImpostor()) SetPlayerNameColor(p, ImpostorRed);
                break;

            case RoleType.Jackal:
            case RoleType.Sidekick:
                SetPlayerNameColor(p, JackalBlue);
                foreach (var jk in Jackal.AllPlayers) SetPlayerNameColor(jk, JackalBlue);
                foreach (var sk in Sidekick.AllPlayers) SetPlayerNameColor(sk, JackalBlue);
                break;
        }
    }

    [HarmonyPatch(nameof(HudManager.OpenMeetingRoom)), HarmonyPrefix]
    internal static void OpenMeetingRoom(HudManager __instance)
    {
        SyncMeeting.StartMeeting();
    }
}
