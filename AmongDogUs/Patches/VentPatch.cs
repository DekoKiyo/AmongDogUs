namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(Vent))]
internal static class VentPatch
{
    [HarmonyPatch(nameof(Vent.CanUse)), HarmonyPrefix]
    internal static bool CanUse(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        float Num = float.MaxValue;
        PlayerControl @object = pc.Object;
        bool roleCouldUse = @object.RoleCanUseVents();

        var usableDistance = __instance.UsableDistance;
        if (__instance.name.StartsWith("SealedVent_"))
        {
            canUse = couldUse = false;
            __result = Num;
            return false;
        }

        // Submerged Compatibility if needed:
        if (SubmergedCompatibility.IsSubmerged)
        {
            // as submerged does, only change stuff for vents 9 and 14 of submerged. Code partially provided by AlexejheroYTB
            if (SubmergedCompatibility.GetInTransition())
            {
                __result = float.MaxValue;
                return canUse = couldUse = false;
            }
            switch (__instance.Id)
            {
                case 9:  // Cannot enter vent 9 (Engine Room Exit Only Vent)!
                    if (CachedPlayer.LocalPlayer.PlayerControl.inVent) break;
                    __result = float.MaxValue;
                    return canUse = couldUse = false;
                case 14: // Lower Central
                    __result = float.MaxValue;
                    couldUse = roleCouldUse && !pc.IsDead && (@object.CanMove || @object.inVent);
                    canUse = couldUse;
                    if (canUse)
                    {
                        Vector3 center = @object.Collider.bounds.center;
                        Vector3 position = __instance.transform.position;
                        __result = Vector2.Distance(center, position);
                        canUse &= __result <= __instance.UsableDistance;
                    }
                    return false;
            }
        }

        couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
        canUse = couldUse;
        if (canUse)
        {
            Vector2 truePosition = @object.GetTruePosition();
            Vector3 position = __instance.transform.position;
            Num = Vector2.Distance(truePosition, position);

            canUse &= Num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
        }
        __result = Num;
        return false;
    }

    [HarmonyPatch(nameof(Vent.Use)), HarmonyPrefix]
    internal static bool Use(Vent __instance)
    {
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out _);
        bool CannotMoveInVents = (PlayerControl.LocalPlayer.IsRole(RoleType.Madmate) && !Madmate.CanMoveInVents) || (PlayerControl.LocalPlayer.IsRole(RoleType.Jester) && !Jester.CanMoveInVents);
        if (!canUse) return false; // No need to execute the native method as using is disallowed anyways
        bool isEnter = !PlayerControl.LocalPlayer.inVent;

        if (isEnter) PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);
        else PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);

        __instance.SetButtons(isEnter && !CannotMoveInVents);
        return false;
    }

    internal static bool ShuffleTimingIsEnterVents = false;

    [HarmonyPatch(nameof(Vent.EnterVent)), HarmonyPostfix]
    internal static void Postfix()
    {
        if (AirShipPatch.IsShuffleElecDoors && ShuffleTimingIsEnterVents) AirShipPatch.InitializeElecDoor();
        UnderTaker.OnEnterVent();
    }
}