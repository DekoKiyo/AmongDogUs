namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(MainMenuManager))]
internal static class MainMenuManagerPatch
{
    private static bool AssetsLoaded = false;
    [HarmonyPatch(nameof(MainMenuManager.Start)), HarmonyPrefix]
    internal static void LoadAssets()
    {
        if (!AssetsLoaded)
        {
            ModAssets.LoadAssets();
            AssetsLoaded = true;
        }
        ModMapOptions.ReloadPluginOptions();
        AirShipOption.Load();
    }
}