namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(MeetingHud))]
internal static class MeetingHudPatch
{
    [HarmonyPatch(nameof(MeetingHud.BloopAVoteIcon)), HarmonyPrefix]
    internal static bool BloopAVoteIcon(MeetingHud __instance, [HarmonyArgument(0)] GameData.PlayerInfo voterPlayer, [HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
    {
        SpriteRenderer spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab);
        int cId = voterPlayer.DefaultOutfit.ColorId;
        if (!(!GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.AnonymousVotes) || (PlayerControl.LocalPlayer.Data.IsDead && ModMapOptions.GhostsSeeVotes) || PlayerControl.LocalPlayer.HasModifier(ModifierType.Watcher)))
            voterPlayer.Object.SetColor(6);
        voterPlayer.Object.SetPlayerMaterialColors(spriteRenderer);
        spriteRenderer.transform.SetParent(parent);
        spriteRenderer.transform.localScale = Vector3.zero;
        __instance.StartCoroutine(Effects.Bloop(index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
        parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
        voterPlayer.Object.SetColor(cId);
        return false;
    }

    [HarmonyPatch(nameof(MeetingHud.Update)), HarmonyPostfix]
    internal static void Update(MeetingHud __instance)
    {
        if (__instance.state == MeetingHud.VoteStates.Animating) return;

        // Deactivate skip Button if skipping on emergency meetings is disabled
        if (ModMapOptions.BlockSkippingInEmergencyMeetings) __instance.SkipVoteButton?.gameObject?.SetActive(false);

        // This fixes a bug with the original game where pressing the button and a kill happens simultaneously
        // results in bodies sometimes being created *after* the meeting starts, marking them as dead and
        // removing the corpses so there's no random corpses leftover afterwards
        foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
        {
            if (b == null) continue;

            foreach (PlayerVoteArea pva in __instance.playerStates)
            {
                if (pva == null) continue;

                if (pva.TargetPlayerId == b?.ParentId && !pva.AmDead)
                {
                    pva?.SetDead(pva.DidReport, true);
                    pva?.Overlay?.gameObject?.SetActive(true);
                }
            }
        }
    }

    private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
    {
        Dictionary<byte, int> dictionary = new();
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            byte votedFor = playerVoteArea.VotedFor;
            if (votedFor != 252 && votedFor != 255 && votedFor != 254)
            {
                int additionalVotes = 1;
                PlayerControl player = Helpers.PlayerById(playerVoteArea.TargetPlayerId);
                if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;
                foreach (var mayor in Mayor.AllPlayers) additionalVotes = (Mayor.Exists && mayor.PlayerId == playerVoteArea.TargetPlayerId) ? Mayor.NumVotes : 1; // May
                if (dictionary.TryGetValue(votedFor, out int currentVotes)) dictionary[votedFor] = currentVotes + additionalVotes;
                else dictionary[votedFor] = additionalVotes;
            }
        }
        return dictionary;
    }

    [HarmonyPatch(nameof(MeetingHud.CheckForEndVoting)), HarmonyPrefix]
    internal static bool CheckForEndVoting(MeetingHud __instance)
    {
        if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
        {
            Dictionary<byte, int> self = CalculateVotes(__instance);
            KeyValuePair<byte, int> max = self.MaxPair(out bool tie);
            GameData.PlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

            MeetingHud.VoterState[] array = new MeetingHud.VoterState[__instance.playerStates.Length];
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                array[i] = new MeetingHud.VoterState
                {
                    VoterId = playerVoteArea.TargetPlayerId,
                    VotedForId = playerVoteArea.VotedFor
                };
            }

            // RPCVotingComplete
            __instance.RpcVotingComplete(array, exiled, tie);
        }
        return false;
    }

    [HarmonyPatch(nameof(MeetingHud.Select)), HarmonyPrefix]
    internal static bool Select(ref bool __result, MeetingHud __instance, [HarmonyArgument(0)] int suspectStateIdx)
    {
        __result = false;
        if (ModMapOptions.NoVoteIsSelfVote && PlayerControl.LocalPlayer.PlayerId == suspectStateIdx) return false;
        if (ModMapOptions.BlockSkippingInEmergencyMeetings && suspectStateIdx == -1) return false;

        return true;
    }

    [HarmonyPatch(nameof(MeetingHud.PopulateResults)), HarmonyPrefix]
    internal static bool PopulateResults(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states)
    {
        // __instance.TitleText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
        int num = 0;
        Dictionary<int, int> votesApplied = new();
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            byte targetPlayerId = playerVoteArea.TargetPlayerId;

            playerVoteArea.ClearForResults();
            int num2 = 0;
            for (int j = 0; j < states.Length; j++)
            {
                MeetingHud.VoterState voterState = states[j];
                GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                if (playerById == null)
                {
                    Debug.LogError($"Couldn't find player info for voter: {voterState.VoterId}");
                }
                else if (i == 0 && voterState.SkippedVote && !playerById.IsDead)
                {
                    __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                    num++;
                }
                else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead)
                {
                    __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                    num2++;
                }
                foreach (var mayor in Mayor.AllPlayers) if (Mayor.Exists && playerById.PlayerId == mayor.PlayerId && votesApplied[playerById.PlayerId] < Mayor.NumVotes) j--;
            }
        }
        return false;
    }
}