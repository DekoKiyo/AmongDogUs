namespace AmongDogUs.Utilities;

internal class CachedPlayer
{
    internal static readonly Dictionary<IntPtr, CachedPlayer> PlayerPtrs = new();
    internal static readonly List<CachedPlayer> AllPlayers = new();
    internal static CachedPlayer LocalPlayer;

    internal Transform transform;
    internal PlayerControl PlayerControl;
    internal PlayerPhysics PlayerPhysics;
    internal CustomNetworkTransform NetTransform;
    internal GameData.PlayerInfo Data;
    internal byte PlayerId;

    public static implicit operator bool(CachedPlayer player)
    {
        return player != null && player.PlayerControl;
    }

    public static implicit operator PlayerControl(CachedPlayer player) => player.PlayerControl;
    public static implicit operator PlayerPhysics(CachedPlayer player) => player.PlayerPhysics;
}

[HarmonyPatch]
internal static class CachedPlayerPatches
{
    [HarmonyPatch]
    private class CacheLocalPlayerPatch
    {
        [HarmonyTargetMethod]
        internal static MethodBase TargetMethod()
        {
            var type = typeof(PlayerControl).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.Name.Contains("Start"));
            return AccessTools.Method(type, nameof(IEnumerator.MoveNext));
        }

        [HarmonyPostfix]
        internal static void SetLocalPlayer()
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (!localPlayer)
            {
                CachedPlayer.LocalPlayer = null;
                return;
            }

            var cached = CachedPlayer.AllPlayers.FirstOrDefault(p => p.PlayerControl.Pointer == localPlayer.Pointer);
            if (cached != null)
            {
                CachedPlayer.LocalPlayer = cached;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
    [HarmonyPostfix]
    internal static void CachePlayerPatch(PlayerControl __instance)
    {
        if (__instance.notRealPlayer) return;
        var player = new CachedPlayer
        {
            transform = __instance.transform,
            PlayerControl = __instance,
            PlayerPhysics = __instance.MyPhysics,
            NetTransform = __instance.NetTransform
        };
        CachedPlayer.AllPlayers.Add(player);
        CachedPlayer.PlayerPtrs[__instance.Pointer] = player;

#if DEBUG
        foreach (var cachedPlayer in CachedPlayer.AllPlayers)
        {
            if (!cachedPlayer.PlayerControl || !cachedPlayer.PlayerPhysics || !cachedPlayer.NetTransform || !cachedPlayer.transform)
            {
                Main.Logger.LogError($"CachedPlayer {cachedPlayer.PlayerControl.name} has null fields");
            }
        }
#endif
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
    [HarmonyPostfix]
    internal static void RemoveCachedPlayerPatch(PlayerControl __instance)
    {
        if (__instance.notRealPlayer) return;
        CachedPlayer.AllPlayers.RemoveAll(p => p.PlayerControl.Pointer == __instance.Pointer);
        CachedPlayer.PlayerPtrs.Remove(__instance.Pointer);
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.Deserialize))]
    [HarmonyPostfix]
    internal static void AddCachedDataOnDeserialize()
    {
        foreach (CachedPlayer cachedPlayer in CachedPlayer.AllPlayers)
        {
            cachedPlayer.Data = cachedPlayer.PlayerControl.Data;
            cachedPlayer.PlayerId = cachedPlayer.PlayerControl.PlayerId;
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.AddPlayer))]
    [HarmonyPostfix]
    internal static void AddCachedDataOnAddPlayer()
    {
        foreach (CachedPlayer cachedPlayer in CachedPlayer.AllPlayers)
        {
            cachedPlayer.Data = cachedPlayer.PlayerControl.Data;
            cachedPlayer.PlayerId = cachedPlayer.PlayerControl.PlayerId;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Deserialize))]
    [HarmonyPostfix]
    internal static void SetCachedPlayerId(PlayerControl __instance)
    {
        CachedPlayer.PlayerPtrs[__instance.Pointer].PlayerId = __instance.PlayerId;
    }
}