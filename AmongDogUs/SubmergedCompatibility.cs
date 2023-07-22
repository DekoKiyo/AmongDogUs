// Source Code from TheOtherRoles (https://github.com/TheOtherRolesAU/TheOtherRoles)

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

    internal static MonoBehaviour SubmarineStatus { get; private set; }

    internal static bool IsSubmerged { get; private set; }

    internal static bool DisableO2MaskCheckForEmergency
    {
        set
        {
            if (!Loaded) return;
            DisableO2MaskCheckField.SetValue(null, value);
        }
    }

    internal static void SetupMap(ShipStatus map)
    {
        if (map == null)
        {
            IsSubmerged = false;
            SubmarineStatus = null;
            return;
        }

        IsSubmerged = map.Type == SUBMERGED_MAP_TYPE;
        if (!IsSubmerged) return;

        SubmarineStatus = map.GetComponent(Il2CppType.From(SubmarineStatusType))?.TryCast(SubmarineStatusType) as MonoBehaviour;
    }

    private static Type SubmarineStatusType;
    private static MethodInfo CalculateLightRadiusMethod;

    private static Type TaskIsEmergencyPatchType;
    private static FieldInfo DisableO2MaskCheckField;

    private static MethodInfo RpcRequestChangeFloorMethod;
    private static Type FloorHandlerType;
    private static MethodInfo GetFloorHandlerMethod;

    private static Type VentMoveToVentPatchType;
    private static FieldInfo InTransitionField;

    internal static TaskTypes RetrieveOxygenMask = (TaskTypes)152;
    private static Type SubmarineOxygenSystemType;
    private static MethodInfo SubmarineOxygenSystemInstanceField;
    private static MethodInfo RepairDamageMethod;

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

        SubmarineStatusType = Types.First(t => t.Name == "SubmarineStatus");
        CalculateLightRadiusMethod = AccessTools.Method(SubmarineStatusType, "CalculateLightRadius");

        TaskIsEmergencyPatchType = Types.First(t => t.Name == "PlayerTaskTaskIsEmergencyPatch");
        DisableO2MaskCheckField = AccessTools.Field(TaskIsEmergencyPatchType, "disableO2MaskCheck");

        FloorHandlerType = Types.First(t => t.Name == "FloorHandler");
        GetFloorHandlerMethod = AccessTools.Method(FloorHandlerType, "GetFloorHandler", new Type[] { typeof(PlayerControl) });
        RpcRequestChangeFloorMethod = AccessTools.Method(FloorHandlerType, "RpcRequestChangeFloor");

        VentMoveToVentPatchType = Types.First(t => t.Name == "VentMoveToVentPatch");
        InTransitionField = AccessTools.Field(VentMoveToVentPatchType, "inTransition");

        SubmarineOxygenSystemType = Types.First(t => t.Name == "SubmarineOxygenSystem" && t.Namespace == "Submerged.Systems.Oxygen");
        SubmarineOxygenSystemInstanceField = AccessTools.PropertyGetter(SubmarineOxygenSystemType, "Instance");
        RepairDamageMethod = AccessTools.Method(SubmarineOxygenSystemType, "RepairDamage");
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
        return (float)CalculateLightRadiusMethod.Invoke(SubmarineStatus, new object[] { null, true, isImpostor });
    }

    internal static void ChangeFloor(bool toUpper)
    {
        if (!Loaded) return;
        MonoBehaviour _floorHandler = ((Component)GetFloorHandlerMethod.Invoke(null, new object[] { CachedPlayer.LocalPlayer.PlayerControl })).TryCast(FloorHandlerType) as MonoBehaviour;
        RpcRequestChangeFloorMethod.Invoke(_floorHandler, new object[] { toUpper });
    }

    internal static bool GetInTransition()
    {
        if (!Loaded) return false;
        return (bool)InTransitionField.GetValue(null);
    }

    internal static void RepairOxygen()
    {
        if (!Loaded) return;
        try
        {
            ShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);
            RepairDamageMethod.Invoke(SubmarineOxygenSystemInstanceField.Invoke(null, Array.Empty<object>()), new object[] { CachedPlayer.LocalPlayer.PlayerControl, 64 });
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