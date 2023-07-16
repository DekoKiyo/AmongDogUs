namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(VersionShower))]
internal static class VersionShowerPatch
{
    internal static TextMeshPro ModVersionShower;

    [HarmonyPatch(nameof(VersionShower.Start)), HarmonyPostfix]
    internal static void GenerateModVersion(VersionShower __instance)
    {
        ModVersionShower = Object.Instantiate(__instance.text, __instance.transform);
        ModVersionShower.text = string.Format(ModResources.VersionShower, Main.PLUGIN_VERSION);
        ModVersionShower.transform.localPosition = new(9, 0, -5);

        OnlineMenu.DisableOnline(__instance);
    }
}