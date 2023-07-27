// Source Code from TheOtherRoles (https://github.com/TheOtherRolesAU/TheOtherRoles)
// Edited by DekoKiyo

using Submerged.Map;
using Submerged.Floors;
using Submerged.Vents;
using Submerged.Systems.Oxygen;
using Submerged.Enums;

namespace AmongDogUs;

internal static class SubmergedCompatibility
{
    internal const string ELEVATOR_MOVER = "ElevatorMover";
    internal const ShipStatus.MapType SUBMERGED_MAP_TYPE = (ShipStatus.MapType)5;

    internal static SemanticVersioning.Version Version { get; private set; }
    internal static bool Loaded { get; private set; }
    internal static bool LoadedExternally { get; private set; }
    internal static BasePlugin Plugin { get; private set; }
    internal static Assembly Assembly { get; private set; }
    internal static Type[] Types { get; private set; }
    internal static Dictionary<string, Type> InjectedTypes { get; private set; }
    internal static TaskTypes RetrieveOxygenMask;

    internal static SubmarineStatus SubmarineStatusBehaviour { get; private set; }

    internal static bool IsSubmerged { get; private set; }

    internal static void SetupMap(ShipStatus map)
    {
        if (map == null)
        {
            IsSubmerged = false;
            SubmarineStatusBehaviour = null;
            return;
        }

        IsSubmerged = map.Type == SUBMERGED_MAP_TYPE;
        if (!IsSubmerged) return;

        SubmarineStatusBehaviour = map.GetComponent(Il2CppType.From(typeof(SubmarineStatus)))?.TryCast<SubmarineStatus>();
    }

    internal static bool TryLoadSubmerged()
    {
        try
        {
            Main.Logger.LogMessage("Trying to load Submerged...");
            var thisAsm = Assembly.GetCallingAssembly();
            var resourceName = thisAsm.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith("Submerged.dll"));
            if (resourceName == default) return false;

            using var submergedStream = thisAsm.GetManifestResourceStream(resourceName)!;
            byte[] assemblyBuffer = new byte[submergedStream.Length];
            submergedStream.Read(assemblyBuffer, 0, assemblyBuffer.Length);
            Assembly = Assembly.Load(assemblyBuffer);

            var pluginType = Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(BasePlugin)));
            Plugin = (BasePlugin)Activator.CreateInstance(pluginType!);
            Plugin.Load();

            Version = pluginType.GetCustomAttribute<BepInPlugin>().Version.BaseVersion(); ;

            IL2CPPChainloader.Instance.Plugins[Main.SUBMERGED_GUID] = new();
            return true;
        }
        catch (Exception e)
        {
            Main.Logger.LogError(e);
        }
        return false;
    }

    internal static void Initialize()
    {
        Loaded = IL2CPPChainloader.Instance.Plugins.TryGetValue(Main.SUBMERGED_GUID, out PluginInfo plugin);
        if (!Loaded)
        {
            if (TryLoadSubmerged()) Loaded = true;
            else return;
        }
        else
        {
            LoadedExternally = true;
            Plugin = plugin!.Instance as BasePlugin;
            Version = plugin.Metadata.Version.BaseVersion();
            Assembly = Plugin!.GetType().Assembly;
        }

        Types = AccessTools.GetTypesFromAssembly(Assembly);
        InjectedTypes = (Dictionary<string, Type>)AccessTools.PropertyGetter(Types.FirstOrDefault(t => t.Name == "ComponentExtensions"), "RegisteredTypes").Invoke(null, Array.Empty<object>());
        RetrieveOxygenMask = CustomTaskTypes.RetrieveOxygenMask;
    }

    internal static MonoBehaviour AddSubmergedComponent(this GameObject obj, string typeName)
    {
        if (!Loaded) return obj.AddComponent<MissingSubmergedBehaviour>();
        bool validType = InjectedTypes.TryGetValue(typeName, out Type type);
        return validType ? obj.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>() : obj.AddComponent<MissingSubmergedBehaviour>();
    }

    internal static float GetSubmergedNeutralLightRadius(bool isImpostor)
    {
        if (!Loaded) return 0;
        return SubmarineStatusBehaviour.CalculateLightRadius(null, true, isImpostor);
    }

    internal static void ChangeFloor(bool toUpper)
    {
        if (!Loaded) return;
        var _floorHandler = FloorHandler.GetFloorHandler(CachedPlayer.LocalPlayer.PlayerControl);
        _floorHandler.RpcRequestChangeFloor(toUpper);
    }

    internal static bool GetInTransition()
    {
        if (!Loaded) return false;
        return VentPatchData.InTransition;
    }

    internal static void RepairOxygen()
    {
        if (!Loaded) return;
        try
        {
            ShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);
            SubmarineOxygenSystem.Instance.RepairDamage(CachedPlayer.LocalPlayer.PlayerControl, 64);
        }
        catch (NullReferenceException)
        {
            Main.Logger.LogMessage("null reference in engineer oxygen fix");
        }
    }
}

internal class MissingSubmergedBehaviour : MonoBehaviour
{
    static MissingSubmergedBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<MissingSubmergedBehaviour>();
    internal MissingSubmergedBehaviour(IntPtr ptr) : base(ptr) { }
}