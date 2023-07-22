namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(RoleOptionsCollectionV07))]
internal static class RoleOptionsDataGetNumPerGamePatch
{
    [HarmonyPatch(nameof(RoleOptionsCollectionV07.GetNumPerGame)), HarmonyPostfix]
    internal static void GetNumPerGame(ref int __result)
    {
        if (Helpers.RolesEnabled && GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.Normal) __result = 0;
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions))]
internal static class GameOptionsDataGetAdjustedNumImpostorsPatch
{
    [HarmonyPatch(nameof(IGameOptionsExtensions.GetAdjustedNumImpostors)), HarmonyPostfix]
    internal static void GetAdjustedNumImpostors(ref int __result)
    {
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
        {
            // Ignore Vanilla impostor limits.
            __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 1, 3);
        }
    }
}

[HarmonyPatch(typeof(GameOptionsData))]
internal static class GameOptionsDataValidatePatch
{
    [HarmonyPatch(nameof(GameOptionsData.Validate)), HarmonyPostfix]
    internal static void Validate(GameOptionsData __instance)
    {
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal) return;
        __instance.NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
    }
}

[HarmonyPatch(typeof(RoleManager))]
class RoleManagerSelectRolesPatch
{
    private static (int rate, int count) crewValues;
    private static (int rate, int count) impValues;
    private static readonly List<Tuple<byte, byte>> playerRoleMap = new();

    [HarmonyPatch(nameof(RoleManager.SelectRoles)), HarmonyPostfix]
    internal static void SelectRoles()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)ECustomRPC.ResetVariables, SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.ResetVariables();
        // Don't assign Roles in Tutorial or if deactivated
        if (Helpers.RolesEnabled) AssignRoles();
    }

    private static void AssignRoles()
    {
        var data = GetRoleAssignmentData();
        AssignSpecialRoles(data); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
        SelectFactionForFactionIndependentRoles(data);
        AssignEnsuredRoles(data); // Assign roles that should always be in the game next
        AssignChanceRoles(data); // Assign roles that may or may not be in the game last
        AssignModifiers(data); // MODIFIERS
    }

    internal static RoleAssignmentData GetRoleAssignmentData()
    {
        // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
        List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
        List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

        var crewmateMin = CustomOptionsHolder.CrewmateRolesCountMin.GetSelection();
        var crewmateMax = CustomOptionsHolder.CrewmateRolesCountMax.GetSelection();
        var neutralMin = CustomOptionsHolder.NeutralRolesCountMin.GetSelection();
        var neutralMax = CustomOptionsHolder.NeutralRolesCountMax.GetSelection();
        var impostorMin = CustomOptionsHolder.ImpostorRolesCountMin.GetSelection();
        var impostorMax = CustomOptionsHolder.ImpostorRolesCountMax.GetSelection();

        // Make sure min is less or equal to max
        if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
        if (neutralMin > neutralMax) neutralMin = neutralMax;
        if (impostorMin > impostorMax) impostorMin = impostorMax;

        // Automatically force everyone to get a role by setting crew Min / Max according to Neutral Settings
        if (CustomOptionsHolder.FillCrewmate.GetBool())
        {
            crewmateMax = crewmates.Count - neutralMin;
            crewmateMin = crewmates.Count - neutralMax;
        }

        // Get the maximum allowed count of each role type based on the minimum and maximum option
        int crewCountSettings = Main.Random.Next(crewmateMin, crewmateMax + 1);
        int neutralCountSettings = Main.Random.Next(neutralMin, neutralMax + 1);
        int impCountSettings = Main.Random.Next(impostorMin, impostorMax + 1);

        // Potentially lower the actual maximum to the assignable players
        int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
        int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
        int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

        // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
        Dictionary<byte, (int rate, int count)> ImpSettings = new();
        Dictionary<byte, (int rate, int count)> NeutralSettings = new();
        Dictionary<byte, (int rate, int count)> CrewSettings = new();

        ImpSettings.Add((byte)Roles.RoleType.CustomImpostor, CustomOptionsHolder.CustomImpostorRate.Data);
        ImpSettings.Add((byte)Roles.RoleType.UnderTaker, CustomOptionsHolder.UnderTakerRate.Data);
        ImpSettings.Add((byte)Roles.RoleType.BountyHunter, CustomOptionsHolder.BountyHunterRate.Data);
        ImpSettings.Add((byte)Roles.RoleType.Teleporter, CustomOptionsHolder.TeleporterRate.Data);
        ImpSettings.Add((byte)Roles.RoleType.EvilHacker, CustomOptionsHolder.EvilHackerRate.Data);

        NeutralSettings.Add((byte)Roles.RoleType.Jester, CustomOptionsHolder.JesterRate.Data);
        NeutralSettings.Add((byte)Roles.RoleType.Jackal, CustomOptionsHolder.JackalRate.Data);
        NeutralSettings.Add((byte)Roles.RoleType.Arsonist, CustomOptionsHolder.ArsonistRate.Data);

        CrewSettings.Add((byte)Roles.RoleType.Sheriff, CustomOptionsHolder.SheriffRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.ProEngineer, CustomOptionsHolder.EngineerRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.Madmate, CustomOptionsHolder.MadmateRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.Bakery, CustomOptionsHolder.BakeryRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.Snitch, CustomOptionsHolder.SnitchRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.Seer, CustomOptionsHolder.SeerRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.Lighter, CustomOptionsHolder.LighterRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.Altruist, CustomOptionsHolder.AltruistRate.Data);
        CrewSettings.Add((byte)Roles.RoleType.Mayor, CustomOptionsHolder.MayorRate.Data);

        return new RoleAssignmentData
        {
            Crewmates = crewmates,
            Impostors = impostors,
            crewSettings = CrewSettings,
            neutralSettings = NeutralSettings,
            impSettings = ImpSettings,
            MaxCrewmateRoles = maxCrewmateRoles,
            MaxNeutralRoles = maxNeutralRoles,
            MaxImpostorRoles = maxImpostorRoles
        };
    }

    private static void AssignSpecialRoles(RoleAssignmentData data)
    {
        // Assign Mafia
        // if (data.impostors.Count >= 3 && data.maxImpostorRoles >= 3 && (rnd.Next(1, 101) <= CustomOptionsHolder.mafiaSpawnRate.getSelection() * 10))
        // {
        //     setRoleToRandomPlayer((byte)RoleId.Godfather, data.impostors);
        //     setRoleToRandomPlayer((byte)RoleId.Janitor, data.impostors);
        //     setRoleToRandomPlayer((byte)RoleId.Mafioso, data.impostors);
        //     data.maxImpostorRoles -= 3;
        // }
    }

    private static void SelectFactionForFactionIndependentRoles(RoleAssignmentData data)
    {
        // // Assign Guesser (chance to be impostor based on setting)
        // isEvilGuesser = rnd.Next(1, 101) <= CustomOptionsHolder.guesserIsImpGuesserRate.getSelection() * 10;
        // if ((CustomOptionsHolder.guesserSpawnBothRate.getSelection() > 0 &&
        //     CustomOptionsHolder.guesserSpawnRate.getSelection() == 10) ||
        //     CustomOptionsHolder.guesserSpawnBothRate.getSelection() == 0)
        // {
        //     if (isEvilGuesser) data.impSettings.Add((byte)RoleId.EvilGuesser, CustomOptionsHolder.guesserSpawnRate.getSelection());
        //     else data.crewSettings.Add((byte)RoleId.NiceGuesser, CustomOptionsHolder.guesserSpawnRate.getSelection());
        // }
    }

    private static void AssignEnsuredRoles(RoleAssignmentData data)
    {
        // Get all roles where the chance to occur is set to 100%
        List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value.rate == 10).Select(x => x.Key).ToList();
        List<byte> ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value.rate == 10).Select(x => x.Key).ToList();
        List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value.rate == 10).Select(x => x.Key).ToList();

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while (
            (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) ||
            (data.Crewmates.Count > 0 && (
                (data.MaxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) ||
                (data.MaxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
            )))
        {

            Dictionary<RoleType, List<byte>> rolesToAssign = new();
            if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
            if (data.Crewmates.Count > 0 && data.MaxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
            if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

            // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role)
            // then select one of the roles from the selected pool to a player
            // and remove the role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(Main.Random.Next(0, rolesToAssign.Keys.Count));
            var players = roleType == RoleType.Crewmate || roleType == RoleType.Neutral ? data.Crewmates : data.Impostors;
            var index = Main.Random.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            SetRoleToRandomPlayer(rolesToAssign[roleType][index], players);
            rolesToAssign[roleType].RemoveAt(index);

            if (CustomOptionsHolder.BlockedRolePairings.ContainsKey(roleId))
            {
                foreach (var blockedRoleId in CustomOptionsHolder.BlockedRolePairings[roleId])
                {
                    // Set chance for the blocked roles to 0 for chances less than 100%
                    if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = (0, 0);
                    if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = (0, 0);
                    if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = (0, 0);
                    // Remove blocked roles even if the chance was 100%
                    foreach (var ensuredRolesList in rolesToAssign.Values)
                    {
                        ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                    }
                }
            }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate: data.MaxCrewmateRoles--; crewValues.rate -= 10; break;
                case RoleType.Neutral: data.MaxNeutralRoles--; break;
                case RoleType.Impostor: data.MaxImpostorRoles--; impValues.rate -= 10; break;
            }
        }
    }

    private static void AssignChanceRoles(RoleAssignmentData data)
    {
        // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
        List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
        List<byte> neutralTickets = data.neutralSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
        List<byte> impostorTickets = data.impSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while (
            (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && impostorTickets.Count > 0) ||
            (data.Crewmates.Count > 0 && (
                (data.MaxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                (data.MaxNeutralRoles > 0 && neutralTickets.Count > 0)
            )))
        {

            Dictionary<RoleType, List<byte>> rolesToAssign = new();
            if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
            if (data.Crewmates.Count > 0 && data.MaxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(RoleType.Neutral, neutralTickets);
            if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(RoleType.Impostor, impostorTickets);

            // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role)
            // then select one of the roles from the selected pool to a player
            // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(Main.Random.Next(0, rolesToAssign.Keys.Count));
            var players = roleType == RoleType.Crewmate || roleType is RoleType.Neutral ? data.Crewmates : data.Impostors;
            var index = Main.Random.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            SetRoleToRandomPlayer(roleId, players);
            rolesToAssign[roleType].RemoveAll(x => x == roleId);

            if (CustomOptionsHolder.BlockedRolePairings.ContainsKey(roleId))
            {
                foreach (var blockedRoleId in CustomOptionsHolder.BlockedRolePairings[roleId])
                {
                    // Remove tickets of blocked roles from all pools
                    crewmateTickets.RemoveAll(x => x == blockedRoleId);
                    neutralTickets.RemoveAll(x => x == blockedRoleId);
                    impostorTickets.RemoveAll(x => x == blockedRoleId);
                }
            }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate: data.MaxCrewmateRoles--; break;
                case RoleType.Neutral: data.MaxNeutralRoles--; break;
                case RoleType.Impostor: data.MaxImpostorRoles--; break;
            }
        }
    }

    private static void AssignModifiers(RoleAssignmentData data)
    {
        // Opportunist
        for (int i = 0; i < CustomOptionsHolder.OpportunistRate.Count; i++)
        {
            if (Main.Random.Next(1, 100) <= CustomOptionsHolder.OpportunistRate.Rate * 10)
            {
                var candidates = Opportunist.Candidates;
                if (candidates.Count <= 0)
                {
                    break;
                }
                SetModifierToRandomPlayer((byte)ModifierType.Opportunist, Opportunist.Candidates);
            }
        }

        // Sunglasses
        for (int i = 0; i < CustomOptionsHolder.SunglassesRate.Count; i++)
        {
            if (Main.Random.Next(1, 100) <= CustomOptionsHolder.SunglassesRate.Rate * 10)
            {
                var candidates = Sunglasses.Candidates;
                if (candidates.Count <= 0)
                {
                    break;
                }
                SetModifierToRandomPlayer((byte)ModifierType.Sunglasses, Sunglasses.Candidates);
            }
        }

        // Watcher
        for (int i = 0; i < CustomOptionsHolder.WatcherRate.Count; i++)
        {
            if (Main.Random.Next(1, 100) <= CustomOptionsHolder.WatcherRate.Rate * 10)
            {
                var candidates = Watcher.Candidates;
                if (candidates.Count <= 0)
                {
                    break;
                }
                SetModifierToRandomPlayer((byte)ModifierType.Watcher, Watcher.Candidates);
            }
        }
    }

    private static byte SetRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, bool removePlayer = true)
    {
        var index = Main.Random.Next(0, playerList.Count);
        byte playerId = playerList[index].PlayerId;
        if (removePlayer) playerList.RemoveAt(index);

        playerRoleMap.Add(new Tuple<byte, byte>(playerId, roleId));

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)ECustomRPC.SetRole, SendOption.Reliable, -1);
        writer.Write(roleId);
        writer.Write(playerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetRole(roleId, playerId);
        return playerId;
    }

    private static byte SetModifierToRandomPlayer(byte modId, List<PlayerControl> playerList)
    {
        if (playerList.Count <= 0) return byte.MaxValue;

        var index = Main.Random.Next(0, playerList.Count);
        byte playerId = playerList[index].PlayerId;

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.AddModifier, SendOption.Reliable, -1);
        writer.Write(modId);
        writer.Write(playerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.AddModifier(modId, playerId);
        return playerId;
    }

    internal class RoleAssignmentData
    {
        internal List<PlayerControl> Crewmates { get; set; }
        internal List<PlayerControl> Impostors { get; set; }
        internal Dictionary<byte, (int rate, int count)> impSettings = new();
        internal Dictionary<byte, (int rate, int count)> neutralSettings = new();
        internal Dictionary<byte, (int rate, int count)> crewSettings = new();
        internal int MaxCrewmateRoles { get; set; }
        internal int MaxNeutralRoles { get; set; }
        internal int MaxImpostorRoles { get; set; }
    }

    private enum RoleType
    {
        Crewmate = 0,
        Neutral = 1,
        Impostor = 2
    }
}