namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(MapBehaviour))]
internal static class MapBehaviorPatch
{
    internal static Dictionary<byte, SpriteRenderer> mapIcons = null;
    internal static Dictionary<byte, SpriteRenderer> corpseIcons = null;

    internal static Sprite corpseSprite;

    internal static Sprite GetCorpseSprite()
    {
        if (corpseSprite) return corpseSprite;
        corpseSprite = Helpers.LoadSpriteFromTexture2D(ModAssets.DeadBodySprite, 115f);
        return corpseSprite;
    }

    internal static void ResetIcons()
    {
        if (mapIcons != null)
        {
            foreach (SpriteRenderer r in mapIcons.Values) Object.Destroy(r.gameObject);
            mapIcons.Clear();
            mapIcons = null;
        }

        if (corpseIcons != null)
        {
            foreach (SpriteRenderer r in corpseIcons.Values) Object.Destroy(r.gameObject);
            corpseIcons.Clear();
            corpseIcons = null;
        }
    }

    [HarmonyPatch(nameof(MapBehaviour.FixedUpdate)), HarmonyPrefix]
    internal static bool FixedUpdate(MapBehaviour __instance)
    {
        if (!MeetingHud.Instance) return true;  // Only run in meetings, and then set the Position of the HerePoint to the Position before the Meeting!
        // if (!ShipStatus.Instance) {
        //     return false;
        // }
        PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
        return false;
    }


    [HarmonyPatch(nameof(MapBehaviour.ShowNormalMap)), HarmonyPrefix]
    internal static bool ShowNormalMap(MapBehaviour __instance)
    {
        if (!MeetingHud.Instance || __instance.IsOpen) return true;  // Only run in meetings and when the map is closed

        PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
        __instance.GenericShow();
        __instance.taskOverlay.Show();
        __instance.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
        FastDestroyableSingleton<HudManager>.Instance.SetHudActive(false);
        return false;
    }

    [HarmonyPatch(nameof(MapBehaviour.Close)), HarmonyPostfix]
    internal static void Close(MapBehaviour __instance)
    {
        FastDestroyableSingleton<HudManager>.Instance.transform.FindChild("TaskDisplay").FindChild("TaskPanel").gameObject.SetActive(true);
    }
}