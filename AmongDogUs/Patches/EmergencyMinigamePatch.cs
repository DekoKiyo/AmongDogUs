namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(EmergencyMinigame))]
internal static class EmergencyMinigamePatch
{
    [HarmonyPatch(nameof(EmergencyMinigame.Update)), HarmonyPostfix]
    internal static void Update(EmergencyMinigame __instance)
    {
        var roleCanCallEmergency = true;
        var statusText = "";

        // Jester
        if (PlayerControl.LocalPlayer.IsRole(RoleType.Jester) && !Jester.CanCallEmergency)
        {
            roleCanCallEmergency = false;
            statusText = ModResources.JesterMeetingButton;
        }

        if (!roleCanCallEmergency)
        {
            __instance.StatusText.text = statusText;
            __instance.NumberText.text = string.Empty;
            __instance.ClosedLid.gameObject.SetActive(true);
            __instance.OpenLid.gameObject.SetActive(false);
            __instance.ButtonActive = false;
            return;
        }

        // Handle max Number of meetings
        if (__instance.state == 1)
        {
            int localRemaining = PlayerControl.LocalPlayer.RemainingEmergencies;
            int teamRemaining = Mathf.Max(0, ModMapOptions.MaxNumberOfMeetings - ModMapOptions.MeetingsCount);
            int remaining = Mathf.Min(localRemaining, teamRemaining);
            // int remaining = Mathf.Min(localRemaining, PlayerControl.LocalPlayer.IsRole(RoleId.Mayor) ? 1 : teamRemaining);

            __instance.StatusText.text = string.Format(ModResources.MeetingStatus, PlayerControl.LocalPlayer.name, localRemaining.ToString(), teamRemaining.ToString());
            __instance.NumberText.text = "";
            __instance.ButtonActive = remaining > 0;
            __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
            __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
            return;
        }
    }
}