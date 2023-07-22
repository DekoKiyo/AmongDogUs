namespace AmongDogUs.Roles;

internal enum RoleType
{
    NoRole = 0,
    Impostor,
    Crewmate,

    Sheriff = 10,
    ProEngineer,
    Madmate,
    Bakery,
    Snitch,
    Seer,
    Lighter,
    Altruist,
    Mayor,
    CustomImpostor,
    UnderTaker,
    BountyHunter,
    Teleporter,
    EvilHacker,
    Jester,
    Jackal,
    Sidekick,
    Arsonist,
}

[HarmonyPatch]
internal static class RoleData
{
    internal static void SetCustomButtonCooldowns()
    {
        Role.allRoles.Do(x => x.SetButtonCooldowns());
    }

    internal static void MakeButtons(HudManager hm)
    {
        Role.allRoles.Do(x => x.MakeButtons(hm));
    }

    internal static Dictionary<RoleType, Type> allRoleTypes = new()
    {
        { RoleType.Sheriff, typeof(RoleBase<Sheriff>) },
        { RoleType.ProEngineer, typeof(RoleBase<ProEngineer>) },
        { RoleType.Madmate, typeof(RoleBase<Madmate>) },
        { RoleType.Bakery, typeof(RoleBase<Bakery>) },
        { RoleType.Snitch, typeof(RoleBase<Snitch>) },
        { RoleType.Seer, typeof(RoleBase<Seer>) },
        // { RoleId.Lighter, typeof(RoleBase<Lighter>) },
        { RoleType.Altruist, typeof(RoleBase<Altruist>) },
        { RoleType.Mayor, typeof(RoleBase<Mayor>) },

        // Neutral
        { RoleType.Jester, typeof(RoleBase<Jester>) },
        { RoleType.Jackal, typeof(RoleBase<Jackal>) },
        { RoleType.Sidekick, typeof(RoleBase<Sidekick>) },
        { RoleType.Arsonist, typeof(RoleBase<Arsonist>) },

        // Impostor
        { RoleType.CustomImpostor, typeof(RoleBase<CustomImpostor>) },
        { RoleType.UnderTaker, typeof(RoleBase<UnderTaker>) },
        { RoleType.BountyHunter, typeof(RoleBase<BountyHunter>) },
        { RoleType.Teleporter, typeof(RoleBase<Teleporter>) },
        { RoleType.EvilHacker, typeof(RoleBase<EvilHacker>) },
    };
}

internal abstract class Role
{
    internal static List<Role> allRoles = new();
    internal PlayerControl player;
    internal RoleType roleType;

    internal abstract void OnMeetingStart();
    internal abstract void OnMeetingEnd();
    internal abstract void FixedUpdate();
    internal abstract void OnKill(PlayerControl target);
    internal abstract void OnDeath(PlayerControl killer = null);
    internal abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    internal abstract void MakeButtons(HudManager hm);
    internal abstract void SetButtonCooldowns();
    internal abstract void Clear();

    internal static void ClearAll()
    {
        allRoles = new();
    }
}

[HarmonyPatch]
internal abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
{
    internal static List<T> players = new();
    internal static RoleType RoleType;

    internal void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        allRoles.Add(this);
    }

    internal static T Local { get { return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer); } }
    internal static List<PlayerControl> AllPlayers { get { return players.Select(x => x.player).ToList(); } }
    internal static List<PlayerControl> LivingPlayers { get { return players.Select(x => x.player).Where(x => x.IsAlive()).ToList(); } }
    internal static List<PlayerControl> DeadPlayers { get { return players.Select(x => x.player).Where(x => !x.IsAlive()).ToList(); } }
    internal static bool Exists { get { return Helpers.RolesEnabled && players.Count > 0; } }
    public static T GetRole(PlayerControl player = null)
    {
        player ??= PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }
    public static bool IsRole(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }
    public static void SetRole(PlayerControl player)
    {
        if (!IsRole(player))
        {
            T role = new();
            role.Init(player);
        }
    }
    public static void EraseRole(PlayerControl player)
    {
        players.RemoveAll(x => x.player == player && x.roleType == RoleType);
        allRoles.RemoveAll(x => x.player == player && x.roleType == RoleType);
    }
    public static void SwapRole(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0) players[index].player = p2;
    }
}

internal static class RoleHelpers
{
    internal static bool IsRole(this PlayerControl player, RoleType roleId)
    {
        foreach (var t in RoleData.allRoleTypes)
            if (roleId == t.Key) return (bool)t.Value.GetMethod("IsRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
        return false;
    }

    internal static RoleType GetRoleId(this PlayerControl player)
    {
        foreach (var t in RoleData.allRoleTypes) if (player.IsRole(t.Key)) return t.Key;
        return RoleType.NoRole;
    }

    internal static void SetRole(this PlayerControl player, RoleType roleId)
    {
        foreach (var t in RoleData.allRoleTypes)
        {
            if (roleId == t.Key)
            {
                t.Value.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                return;
            }
        }
    }

    internal static void EraseAllRoles(this PlayerControl player)
    {
        foreach (var t in RoleData.allRoleTypes)
        {
            t.Value.GetMethod("EraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
        }
    }

    internal static void OnKill(this PlayerControl player, PlayerControl target)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnKill(target));
        Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnKill(target));
    }

    internal static void OnDeath(this PlayerControl player, PlayerControl killer)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnDeath(killer));
        Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnDeath(killer));

        RPCProcedure.UpdateMeeting(player.PlayerId, true);
    }
}