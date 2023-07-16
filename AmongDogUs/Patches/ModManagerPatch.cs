namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(ModManager))]
internal static class ModManagerPatch
{
    [HarmonyPatch(nameof(ModManager.LateUpdate)), HarmonyPrefix]
    internal static void Initialize(ModManager __instance)
    {
        __instance.ShowModStamp();
    }
}