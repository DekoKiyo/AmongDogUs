namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(Object))]
internal static class UnityEngineObjectPatch
{
    // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) and SpawnInMinigame of submerged
    [HarmonyPatch(nameof(Object.Destroy), new Type[] { typeof(GameObject) })]
    public static void Prefix(GameObject obj)
    {
        if (!SubmergedCompatibility.IsSubmerged) return;
        if (obj.name.Contains("ExileCutscene"))
        {
            ExileControllerPatch.WrapUpPostfix(ExileControllerPatch.lastExiled);
        }
        // else if (obj.name.Contains("SpawnInMinigame"))
        // {
        //     AntiTeleport.setPosition();
        //     Chameleon.lastMoved.Clear();
        // }
    }
}