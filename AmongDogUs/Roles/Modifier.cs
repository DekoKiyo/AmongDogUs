namespace AmongDogUs.Roles;

internal enum ModifierType
{
    None = 0,

    // 新しく書く場合は一番下へ
    Opportunist,
    Sunglasses,
    Watcher,
}

[HarmonyPatch]
internal static class ModifierData
{
    internal static Dictionary<ModifierType, Type> allModTypes = new()
    {
        { ModifierType.Opportunist, typeof(ModifierBase<Opportunist>) },
        { ModifierType.Watcher, typeof(ModifierBase<Watcher>) },
        { ModifierType.Sunglasses, typeof(ModifierBase<Sunglasses>) },
    };
}

internal abstract class Modifier
{
    internal static List<Modifier> allModifiers = new();
    internal PlayerControl player;
    internal ModifierType modType;

    internal abstract string ModifierPostfix();
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
        allModifiers = new();
    }
}

[HarmonyPatch]
internal abstract class ModifierBase<T> : Modifier where T : ModifierBase<T>, new()
{
    internal static List<T> players = new();
    internal static ModifierType ModType;

    internal void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        allModifiers.Add(this);
    }

    internal static T Local { get { return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer); } }
    internal static List<PlayerControl> AllPlayers { get { return players.Select(x => x.player).ToList(); } }
    internal static List<PlayerControl> LivingPlayers { get { return players.Select(x => x.player).Where(x => x.IsAlive()).ToList(); } }
    internal static List<PlayerControl> DeadPlayers { get { return players.Select(x => x.player).Where(x => !x.IsAlive()).ToList(); } }
    internal static bool Exists { get { return Helpers.RolesEnabled && players.Count > 0; } }
    public static T GetModifier(PlayerControl player = null)
    {
        player ??= PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }
    public static bool HasModifier(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }
    public static void AddModifier(PlayerControl player)
    {
        if (!HasModifier(player))
        {
            T mod = new();
            mod.Init(player);
        }
    }
    public static void EraseModifier(PlayerControl player)
    {
        players.RemoveAll(x => x.player == player && x.modType == ModType);
        allModifiers.RemoveAll(x => x.player == player && x.modType == ModType);
    }
    public static void SwapModifier(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            players[index].player = p2;
        }
    }
}

internal static class ModifierHelpers
{
    internal static string GetModifierPostfixString(this PlayerControl __instance, ModifierType modId)
    {
        foreach (var mod in Modifier.allModifiers)
        {
            foreach (var t in ModifierData.allModTypes) if (modId == t.Key) return mod.ModifierPostfix();
        }
        return "NoData";
    }

    internal static bool HasModifier(this PlayerControl player, ModifierType modId)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (modId == t.Key) return (bool)t.Value.GetMethod("HasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
        }
        return false;
    }

    internal static void AddModifier(this PlayerControl player, ModifierType modId)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (modId == t.Key)
            {
                t.Value.GetMethod("AddModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                return;
            }
        }
    }

    internal static void EraseAllModifiers(this PlayerControl player)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            t.Value.GetMethod("EraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
        }
    }
}