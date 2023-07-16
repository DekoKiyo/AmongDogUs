namespace AmongDogUs.Patches;

internal static class ModManagerPatch
{
    internal static void Initialize()
    {
        FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();
    }
}