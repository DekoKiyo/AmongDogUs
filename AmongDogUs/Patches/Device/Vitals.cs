namespace AmongDogUs.Patches;

[Harmony]
internal static class VitalsPatch
{
    internal static float vitalsTimer = 0f;
    internal static TextMeshPro TimeRemaining;
    private static readonly List<TextMeshPro> hackerTextures = new();

    internal static void ResetData()
    {
        vitalsTimer = 0f;
        if (TimeRemaining != null)
        {
            Object.Destroy(TimeRemaining);
            TimeRemaining = null;
        }
    }

    internal static void UseVitalsTime()
    {
        // Don't waste network traffic if we're out of time.
        if (ModMapOptions.RestrictDevices > 0 && ModMapOptions.RestrictVitalsTime > 0f && PlayerControl.LocalPlayer.IsAlive())
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UseVitalsTime, SendOption.Reliable, -1);
            writer.Write(vitalsTimer);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.UseVitalsTime(vitalsTimer);
        }
        vitalsTimer = 0f;
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
    internal static class VitalsMinigameStartPatch
    {
        internal static void Postfix(VitalsMinigame __instance)
        {
            vitalsTimer = 0f;
            /*
            if (Hacker.hacker != null && PlayerControl.LocalPlayer == Hacker.hacker)
            {
                hackerTexts = new List<TextMeshPro>();
                foreach (VitalsPanel panel in __instance.vitals)
                {
                    TextMeshPro text = UnityEngine.Object.Instantiate(__instance.SabText, panel.transform);
                    hackerTexts.Add(text);
                    UnityEngine.Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                    text.gameObject.SetActive(false);
                    text.transform.localScale = Vector3.one * 0.75f;
                    text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);
                }
            }
            */
        }
    }

    [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
    internal static class VitalsMinigameUpdatePatch
    {
        internal static bool Prefix(VitalsMinigame __instance)
        {
            vitalsTimer += Time.deltaTime;
            if (vitalsTimer > 0.1f)
                UseVitalsTime();

            if (ModMapOptions.RestrictDevices > 0)
            {
                if (TimeRemaining == null)
                {
                    TimeRemaining = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, __instance.transform);
                    TimeRemaining.alignment = TextAlignmentOptions.BottomRight;
                    TimeRemaining.transform.position = Vector3.zero;
                    TimeRemaining.transform.localPosition = new Vector3(1.7f, 4.45f);
                    TimeRemaining.transform.localScale *= 1.8f;
                    TimeRemaining.color = Palette.White;
                }

                if (ModMapOptions.RestrictVitalsTime <= 0f)
                {
                    __instance.Close();
                    return false;
                }

                string timeString = TimeSpan.FromSeconds(ModMapOptions.RestrictVitalsTime).ToString(@"mm\:ss\.ff");
                TimeRemaining.text = string.Format(ModResources.TimeRemaining, timeString);
                TimeRemaining.gameObject.SetActive(true);
            }

            return true;
        }

        internal static void Postfix(VitalsMinigame __instance)
        {
            // Hacker show time since death
            /*if (Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && Hacker.hackerTimer > 0)
            {
                for (int k = 0; k < __instance.vitals.Length; k++)
                {
                    VitalsPanel vitalsPanel = __instance.vitals[k];
                    GameData.PlayerInfo player = GameData.Instance.AllPlayers[k];

                    // Hacker update
                    if (vitalsPanel.IsDead)
                    {
                        DeadPlayer deadPlayer = deadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                        if (deadPlayer != null && deadPlayer.timeOfDeath != null && k < hackerTexts.Count && hackerTexts[k] != null)
                        {
                            float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                            hackerTexts[k].gameObject.SetActive(true);
                            hackerTexts[k].text = Math.Round(timeSinceDeath / 1000) + "s";
                        }
                    }
                }
            }
            else
            {*/
            foreach (TextMeshPro text in hackerTextures)
                if (text != null && text.gameObject != null) text.gameObject.SetActive(false);
            // }
        }
    }
    /*
    [HarmonyPatch]
    class VitalsMinigameClosePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(Minigame).GetMethods().Where(x => x.Name == "Close");
        }

        static void Prefix(Minigame __instance)
        {
            if (__instance is VitalsMinigame)
                UseVitalsTime();
        }
    }
    */
}