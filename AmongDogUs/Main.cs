global using Il2CppInterop.Runtime;
global using Il2CppInterop.Runtime.Attributes;
global using Il2CppInterop.Runtime.InteropTypes;
global using Il2CppInterop.Runtime.InteropTypes.Arrays;
global using Il2CppInterop.Runtime.Injection;

global using BepInEx;
global using BepInEx.Logging;
global using BepInEx.Configuration;
global using BepInEx.Unity.IL2CPP;
global using BepInEx.Unity.IL2CPP.Utils.Collections;
global using HarmonyLib;
global using InnerNet;
global using Hazel;
global using TMPro;
global using Twitch;
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
global using AmongDogUs.Patches;
global using AmongDogUs.Properties;
global using AmongDogUs.Utilities;
global using ModResources = AmongDogUs.Properties.Resources;

namespace AmongDogUs;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency(SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInProcess("Among Us.exe")]
internal class Main : BasePlugin
{
    internal const string PLUGIN_GUID = "con.DekoKiyo.AmongDogUs";
    internal const string PLUGIN_NAME = "AmongDogUs";
    internal const string PLUGIN_VERSION = "1.0.0.0";

    internal const string SUBMERGED_GUID = "Submerged";

    internal static Main Instance { get; set; }
    internal static ManualLogSource Logger { get; set; }
    internal Harmony Harmony { get; } = new(PLUGIN_GUID);

    public override void Load()
    {
        Instance = this;
        Logger = BepInEx.Logging.Logger.CreateLogSource("AmongDogRun");

        // Write here to need
        OnlineMenu.Initialize();

        Harmony.PatchAll();

        Logger.Log(LogLevel.Info, "Initialized!");
    }
}