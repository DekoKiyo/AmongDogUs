using Il2Object = Il2CppSystem.Object;

namespace AmongDogUs.Utilities;

internal static class MapUtilities
{
    private static ShipStatus cachedShipStatus = ShipStatus.Instance;

    internal static void MapDestroyed()
    {
        CachedShipStatus = ShipStatus.Instance;
        _systems.Clear();
    }

    private static readonly Dictionary<SystemTypes, Il2Object> _systems = new();
    internal static Dictionary<SystemTypes, Il2Object> Systems
    {
        get
        {
            if (_systems.Count == 0) GetSystems();
            return _systems;
        }
    }

    internal static ShipStatus CachedShipStatus { get => cachedShipStatus; set => cachedShipStatus = value; }

    private static void GetSystems()
    {
        if (!CachedShipStatus) return;

        var systems = CachedShipStatus.Systems;
        if (systems.Count <= 0) return;

        foreach (var systemTypes in SystemTypeHelpers.AllTypes)
        {
            if (!systems.ContainsKey(systemTypes)) continue;
            _systems[systemTypes] = systems[systemTypes].TryCast<Il2Object>();
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
internal static class ShipStatus_Awake_Patch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    internal static void Postfix(ShipStatus __instance)
    {
        MapUtilities.CachedShipStatus = __instance;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class ShipStatus_OnDestroy_Patch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        MapUtilities.CachedShipStatus = null;
        MapUtilities.MapDestroyed();
    }
}