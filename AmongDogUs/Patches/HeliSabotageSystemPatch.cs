namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(HeliSabotageSystem))]
internal static class HeliSabotageSystemPatch
{
    [HarmonyPatch(nameof(HeliSabotageSystem.RepairDamage)), HarmonyPostfix]
    internal static void RepairDamage(HeliSabotageSystem __instance, byte amount)
    {
        HeliSabotageSystem.Tags tags = (HeliSabotageSystem.Tags)(amount & 240);
        if (tags != HeliSabotageSystem.Tags.ActiveBit)
        {
            if (tags == HeliSabotageSystem.Tags.DamageBit)
            {
                __instance.Countdown = CustomOptionsHolder.AirshipReactorDuration.GetFloat();
            }
        }
    }
}