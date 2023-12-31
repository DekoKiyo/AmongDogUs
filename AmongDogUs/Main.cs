global using Il2CppInterop.Runtime;
global using Il2CppInterop.Runtime.Attributes;
global using Il2CppInterop.Runtime.InteropTypes;
global using Il2CppInterop.Runtime.InteropTypes.Arrays;
global using Il2CppInterop.Runtime.Injection;

global using BepInEx;
global using BepInEx.Logging;
global using BepInEx.Configuration;
global using BepInEx.Bootstrap;
global using BepInEx.Unity.IL2CPP.Utils;
global using BepInEx.Unity.IL2CPP;
global using BepInEx.Unity.IL2CPP.Utils.Collections;
global using HarmonyLib;
global using InnerNet;
global using Hazel;
global using TMPro;
global using Twitch;
global using PowerTools;
global using Mono.Cecil;
global using Assets.CoreScripts;
global using Assets.InnerNet;
global using AmongUs.Data;
global using AmongUs.Data.Legacy;
global using AmongUs.GameOptions;
global using Newtonsoft.Json.Linq;

global using UnityEngine;
global using UnityEngine.Events;
global using UnityEngine.UI;
global using UnityEngine.SceneManagement;
global using Object = UnityEngine.Object;
global using static UnityEngine.UI.Button;
global using Reactor;
global using Reactor.Utilities.Extensions;
global using Reactor.Networking.Attributes;

global using System;
global using System.Reflection;
global using System.Text;
global using System.IO;
global using System.Linq;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq.Expressions;
global using System.Text.RegularExpressions;
global using System.Net;
global using System.Net.Http;
global using System.Threading.Tasks;

global using AmongDogUs.Modules;
global using AmongDogUs.Objects;
global using AmongDogUs.Patches;
global using AmongDogUs.Properties;
global using AmongDogUs.Roles;
global using AmongDogUs.Utilities;
global using static AmongDogUs.ColorDictionary;
global using static AmongDogUs.GameHistory;
global using ModResources = AmongDogUs.Properties.Resources;

namespace AmongDogUs;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency(SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInProcess("Among Us.exe")]
[ReactorModFlags(Reactor.Networking.ModFlags.RequireOnAllClients)]
internal class Main : BasePlugin
{
    internal const string PLUGIN_GUID = "jp.DekoKiyo.AmongDogUs";
    internal const string PLUGIN_NAME = "AmongDogUs";
    internal const string PLUGIN_VERSION = "1.0.0.0";
    internal static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

    internal static readonly int PLUGIN_VERSION_MAJOR = Version.Major;
    internal static readonly int PLUGIN_VERSION_MINOR = Version.Minor;
    internal static readonly int PLUGIN_VERSION_BUILD = Version.Build;
    internal static readonly int PLUGIN_VERSION_REVISION = Version.Revision;

    internal const string SUBMERGED_GUID = "Submerged";

    internal static MersenneTwister Random = new();
    internal static Main Instance { get; set; }
    internal static ManualLogSource Logger { get; set; }
    internal Harmony Harmony { get; } = new(PLUGIN_GUID);

    internal static int OptionsPage = 1;
    internal static ConfigEntry<bool> DebugMode { get; private set; }
    internal static ConfigEntry<bool> GhostsSeeTasks { get; set; }
    internal static ConfigEntry<bool> GhostsSeeRoles { get; set; }
    internal static ConfigEntry<bool> GhostsSeeVotes { get; set; }
    internal static ConfigEntry<bool> ShowRoleSummary { get; set; }
    internal static ConfigEntry<bool> EnableCustomSounds { get; set; }
    internal static ConfigEntry<bool> ShowLighterDarker { get; set; }
    internal static ConfigEntry<string> RoomCodeText { get; set; }

    internal static ConfigEntry<bool> TopLeftVert { get; set; }
    internal static ConfigEntry<bool> TopLeftHort { get; set; }
    internal static ConfigEntry<bool> BottomHort { get; set; }
    internal static ConfigEntry<bool> TopCenterHort { get; set; }
    internal static ConfigEntry<bool> LeftVert { get; set; }
    internal static ConfigEntry<bool> RightVert { get; set; }
    internal static ConfigEntry<bool> TopRightVert { get; set; }
    internal static ConfigEntry<bool> TopRightHort { get; set; }
    internal static ConfigEntry<bool> BottomRightVert { get; set; }
    internal static ConfigEntry<bool> BottomRightHort { get; set; }
    internal static ConfigEntry<bool> LeftDoorTop { get; set; }
    internal static ConfigEntry<bool> LeftDoorBottom { get; set; }

    internal static ConfigEntry<int> AirShipDoorMode { get; set; }
    internal static ConfigEntry<int> AirShipDoorChangeTiming { get; set; }

    public override void Load()
    {
        Instance = this;
        Logger = BepInEx.Logging.Logger.CreateLogSource("AmongDogRun");

        DebugMode = Config.Bind("Develop", "Debug Mode", false);
        GhostsSeeTasks = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
        GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
        GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
        ShowRoleSummary = Config.Bind("Custom", "ShowRoleSummary", true);
        EnableCustomSounds = Config.Bind("Custom", "Enable Custom Sounds", true);
        ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", false);
        RoomCodeText = Config.Bind("Custom", "Streamer Mode Room Code Text", "Among Dog Us");

        TopLeftVert = Config.Bind("MapOption", "4ElecDoorTopLeftVert", true);
        TopLeftHort = Config.Bind("MapOption", "4ElecDoorTopLeftHort", true);
        BottomHort = Config.Bind("MapOption", "4ElecDoorBottomHort", true);
        TopCenterHort = Config.Bind("MapOption", "4ElecDoorTopCenterHort", true);
        LeftVert = Config.Bind("MapOption", "4ElecDoorLeftVert", true);
        RightVert = Config.Bind("MapOption", "4ElecDoorRightVert", true);
        TopRightVert = Config.Bind("MapOption", "4ElecDoorTopRightVert", true);
        TopRightHort = Config.Bind("MapOption", "4ElecDoorTopRightHort", true);
        BottomRightVert = Config.Bind("MapOption", "4ElecDoorBottomRightVert", true);
        BottomRightHort = Config.Bind("MapOption", "4ElecDoorBottomRightHort", true);
        LeftDoorTop = Config.Bind("MapOption", "4ElecDoorLeftDoorTop", true);
        LeftDoorBottom = Config.Bind("MapOption", "4ElecDoorLeftDoorBottom", true);

        AirShipDoorMode = Config.Bind("MapOption", "4ElecDoorMode", 0);
        AirShipDoorChangeTiming = Config.Bind("MapOption", "4ElecDoorChangeTiming", 0);

        // Write here to need
        OnlineMenu.Initialize();
        RoleInfoList.Load();
        ModifierInfoList.Load();
        CustomOptionsHolder.Load();
        CustomColors.Load();
        SubmergedCompatibility.Initialize();
        AddComponent<ModUpdateBehaviour>();
        ClassInjector.RegisterTypeInIl2Cpp(typeof(AirShipOption));

        Harmony.PatchAll();

        Logger.Log(LogLevel.Info, "Initialized!");
    }
}

internal static class AmongDogUs
{
    internal static string GetString(this string key)
    {
        string result = "";
        if (key is not null or "") result = key;
        try
        {
            result = ModResources.ResourceManager.GetString(key);
        }
        catch
        {
            Main.Logger.LogWarning($"{key} is not found.");
        }
        return result;
    }

    internal static void ClearAndReloadRoles()
    {
        Clear();
        Role.ClearAll();
    }

    internal static void FixedUpdate(PlayerControl player)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.FixedUpdate());
        Modifier.allModifiers.DoIf(x => x.player == player, x => x.FixedUpdate());
    }

    internal static void OnMeetingStart()
    {
        Role.allRoles.Do(x => x.OnMeetingStart());
        Modifier.allModifiers.Do(x => x.OnMeetingStart());
    }

    internal static void OnMeetingEnd()
    {
        Role.allRoles.Do(x => x.OnMeetingEnd());
        Modifier.allModifiers.Do(x => x.OnMeetingEnd());

        CustomOverlays.HideInfoOverlay();
        CustomOverlays.HideBlackBG();
    }

    internal static void Clear()
    {
        Role.allRoles.Do(x => x.Clear());
        Modifier.allModifiers.Do(x => x.Clear());
    }
}