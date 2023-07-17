namespace AmongDogUs.Roles;

internal abstract class Modifier
{
    internal static List<Modifier> allModifiers = new();
    internal PlayerControl player;
    internal ModifierId modId;

    internal abstract string ModifierPostfix();
    internal abstract void OnMeetingStart();
    internal abstract void OnMeetingEnd();
    internal abstract void FixedUpdate();
    internal abstract void OnKill(PlayerControl target);
    internal abstract void OnDeath(PlayerControl killer = null);
    internal abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
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
    internal static ModifierId ModId;

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
    internal static T GetModifier(PlayerControl player = null)
    {
        player ??= PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }
    internal static bool HasModifier(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }
    internal static void AddModifier(PlayerControl player)
    {
        if (!HasModifier(player))
        {
            T mod = new();
            mod.Init(player);
        }
    }
    internal static void EraseModifier(PlayerControl player)
    {
        players.RemoveAll(x => x.player == player && x.modId == ModId);
        allModifiers.RemoveAll(x => x.player == player && x.modId == ModId);
    }
    internal static void SwapModifier(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            players[index].player = p2;
        }
    }
}

internal enum ModifierId
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
    internal static Dictionary<ModifierId, Type> allModTypes = new()
    {
        // { ModifierId.Opportunist, typeof(ModifierBase<Opportunist>) },
        // { ModifierId.Watcher, typeof(ModifierBase<Watcher>) },
        // { ModifierId.Sunglasses, typeof(ModifierBase<Sunglasses>) },
    };
}

internal static class ModifierHelpers
{
    internal static string GetModifierPostfixString(this PlayerControl __instance, ModifierId modId)
    {
        foreach (var mod in Modifier.allModifiers)
        {
            foreach (var t in ModifierData.allModTypes)
                if (modId == t.Key) return mod.ModifierPostfix();
        }
        return "NoData";
    }

    internal static bool HasModifier(this PlayerControl player, ModifierId modId)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (modId == t.Key)
            {
                return (bool)t.Value.GetMethod("HasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }
        }
        return false;
    }

    internal static void AddModifier(this PlayerControl player, ModifierId modId)
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