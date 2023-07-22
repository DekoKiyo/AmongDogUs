namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(ShipStatus))]
internal static class ShipStatusPatch
{
    [HarmonyPatch(nameof(ShipStatus.CalculateLightRadius)), HarmonyPrefix]
    internal static bool CalculateLightRadius(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player)
    {
        if (!__instance.Systems.ContainsKey(SystemTypes.Electrical) || GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return true;

        // If player is a role which has Impostor vision
        if (Helpers.HasImpostorVision(player))
        {
            //__result = __instance.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
            __result = GetNeutralLightRadius(__instance, true);
            return false;
        }

        // If player is Lighter with ability active
        if (PlayerControl.LocalPlayer.IsRole(RoleType.Lighter))
        {
            float unLerPed = Mathf.InverseLerp(__instance.MinLightRadius, __instance.MaxLightRadius, GetNeutralLightRadius(__instance, false));
            __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.LighterModeLightsOffVision, __instance.MaxLightRadius * Lighter.LighterModeLightsOnVision, unLerPed);
        }
        // Default light radius
        else __result = GetNeutralLightRadius(__instance, false);

        if (PlayerControl.LocalPlayer.HasModifier(ModifierType.Sunglasses)) __result *= 1f - Sunglasses.Vision * 0.1f;

        return false;
    }

    internal static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor)
    {
        if (SubmergedCompatibility.IsSubmerged) return SubmergedCompatibility.GetSubmergedNeutralLightRadius(isImpostor);

        if (isImpostor) return shipStatus.MaxLightRadius * GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;

        SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
        float lerpValue = switchSystem.Value / 255f;

        return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
    }

    private static int originalNumCommonTasksOption = 0;
    private static int originalNumShortTasksOption = 0;
    private static int originalNumLongTasksOption = 0;
    internal static float originalNumCrewVisionOption = 0;
    internal static float originalNumImpVisionOption = 0;
    internal static float originalNumKillCooldownOption = 0;

    [HarmonyPatch(nameof(ShipStatus.Begin)), HarmonyPrefix]
    internal static bool BeginPrefix(ShipStatus __instance)
    {
        originalNumCommonTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks;
        originalNumShortTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks;
        originalNumLongTasksOption = GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks;

        var commonTaskCount = __instance.CommonTasks.Count;
        var normalTaskCount = __instance.NormalTasks.Count;
        var longTaskCount = __instance.LongTasks.Count;

        if (GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks > commonTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = commonTaskCount;
        if (GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks > normalTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = normalTaskCount;
        if (GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks > longTaskCount) GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = longTaskCount;

        return true;
    }

    [HarmonyPatch(nameof(ShipStatus.Begin)), HarmonyPostfix]
    internal static void BeginPostfix(ShipStatus __instance)
    {
        // Restore original settings after the tasks have been selected
        GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = originalNumCommonTasksOption;
        GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = originalNumShortTasksOption;
        GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = originalNumLongTasksOption;
    }

    internal static void ResetVanillaSettings()
    {
        GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod = originalNumImpVisionOption;
        GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = originalNumCrewVisionOption;
        GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown = originalNumKillCooldownOption;
    }
}