namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(AccountManager))]
internal static class AccountManagerPatch
{
    [HarmonyPatch(nameof(AccountManager.RandomizeName)), HarmonyPrefix]
    internal static bool RandomizeName(AccountManager __instance)
    {
        if (LegacySaveManager.lastPlayerName == null) return true;
        DataManager.Player.Customization.Name = LegacySaveManager.lastPlayerName;
        __instance.accountTab.UpdateNameDisplay();
        return false; // Don't execute original
    }
}