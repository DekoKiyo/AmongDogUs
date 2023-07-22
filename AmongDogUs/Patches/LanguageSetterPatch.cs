namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(LanguageSetter))]
internal static class LanguageSetterPatch
{
    [HarmonyPatch(nameof(LanguageSetter.SetLanguage)), HarmonyPostfix]
    internal static void UpdateTranslations()
    {
        try
        {
            ClientOptionsPatch.UpdateTranslations();
            VanillaOptionsPatch.UpdateTranslations();
        }
        catch
        {
            Main.Logger.LogError("Keys not found.");
        }
    }
}