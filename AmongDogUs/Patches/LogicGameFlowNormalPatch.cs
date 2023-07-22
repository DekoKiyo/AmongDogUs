namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(LogicGameFlowNormal))]
internal static class LogicGameFlowNormalPatch
{
    [HarmonyPatch(nameof(LogicGameFlowNormal.IsGameOverDueToDeath)), HarmonyPostfix]
    internal static void IsGameOverDueToDeath(ShipStatus __instance, ref bool __result)
    {
        __result = false;
    }
}