namespace AmongDogUs.Patches;

[HarmonyPatch(typeof(IntroCutscene))]
internal static class IntroCutscenePatch
{
    internal static PoolablePlayer playerPrefab;
    internal static Vector3 bottomLeft;
    internal static int playerCounter = 0;

    [HarmonyPatch(nameof(IntroCutscene.OnDestroy)), HarmonyPrefix]
    internal static void Prefix(IntroCutscene __instance)
    {
        // Update Role Description
        Helpers.RefreshRoleDescription(PlayerControl.LocalPlayer);

        // Generate and initialize player icons
        if (CachedPlayer.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
        {
            float aspect = Camera.main.aspect;
            float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
            float xPos = 1.75f - safeOrthographicSize * aspect * 1.70f;
            float yPos = 0.15f - safeOrthographicSize * 1.7f;
            bottomLeft = new Vector3(xPos / 2, yPos / 2, -61f);

            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                GameData.PlayerInfo data = p.Data;
                PoolablePlayer player = Object.Instantiate(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                playerPrefab = __instance.PlayerPrefab;
                p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                // PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                player.cosmetics.nameText.text = data.PlayerName;
                player.SetFlipX(true);
                ModMapOptions.PlayerIcons[p.PlayerId] = player;
                player.gameObject.SetActive(false);

                if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleType.Arsonist))
                {
                    player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                    player.transform.localScale = Vector3.one * 0.2f;
                    player.SetSemiTransparent(true);
                    player.gameObject.SetActive(true);
                }
                else
                {
                    // This can be done for all players not just for the bounty hunter as it was before. Allows the thief to have the correct position and scaling
                    player.transform.localPosition = bottomLeft;
                    player.transform.localScale = Vector3.one * 0.4f;
                    player.gameObject.SetActive(false);
                }
            }
        }

        // Force Bounty Hunter to load a new Bounty when the Intro is over
        if (BountyHunter.Bounty != null && CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleType.BountyHunter))
        {
            BountyHunter.BountyUpdateTimer = 0f;
            if (FastDestroyableSingleton<HudManager>.Instance != null)
            {
                BountyHunter.CooldownTimer = Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                BountyHunter.CooldownTimer.alignment = TextAlignmentOptions.Center;
                BountyHunter.CooldownTimer.transform.localPosition = bottomLeft + new Vector3(0f, -0.35f, -62f);
                BountyHunter.CooldownTimer.transform.localScale = Vector3.one * 0.4f;
                BountyHunter.CooldownTimer.gameObject.SetActive(true);
            }
        }
    }

    internal static void SetupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        // Intro solo teams
        if (Helpers.IsNeutral(CachedPlayer.LocalPlayer.PlayerControl))
        {
            var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            soloTeam.Add(CachedPlayer.LocalPlayer.PlayerControl);
            yourTeam = soloTeam;
        }

        // Add the Spy to the Impostor team (for the Impostors)
        // if (Spy.spy != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
        // {
        //     List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        //     var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
        //     fakeImpostorTeam.Add(CachedPlayer.LocalPlayer.PlayerControl);
        //     foreach (PlayerControl p in players)
        //     {
        //         if (CachedPlayer.LocalPlayer.PlayerControl != p && (p == Spy.spy || p.Data.Role.IsImpostor))
        //             fakeImpostorTeam.Add(p);
        //     }
        //     yourTeam = fakeImpostorTeam;
        // }
    }

    internal static void SetupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        List<RoleInfo> infos = RoleInfoList.GetRoleInfoForPlayer(PlayerControl.LocalPlayer);
        RoleInfo roleInfo = infos.Where(info => info.RoleId != RoleType.NoRole).FirstOrDefault();
        if (roleInfo == null) return;
        if (PlayerControl.LocalPlayer.IsNeutral())
        {
            __instance.BackgroundBar.material.color = roleInfo.RoleColor;
            __instance.TeamTitle.text = roleInfo.Name;
            __instance.TeamTitle.color = roleInfo.RoleColor;
            __instance.ImpostorText.text = "";
        }
    }

    internal static IEnumerator<WaitForSeconds> EndShowRole(IntroCutscene __instance)
    {
        yield return new WaitForSeconds(5f);
        __instance.YouAreText.gameObject.SetActive(false);
        __instance.RoleText.gameObject.SetActive(false);
        __instance.RoleBlurbText.gameObject.SetActive(false);
        __instance.ourCrewmate.gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(IntroCutscene.CreatePlayer)), HarmonyPostfix]
    internal static void CreatePlayer(IntroCutscene __instance, bool impostorPositioning, ref PoolablePlayer __result)
    {
        if (impostorPositioning) __result.SetNameColor(ImpostorRed);
    }

    internal static int seed = 0;

    [HarmonyPatch(nameof(IntroCutscene.ShowRole)), HarmonyPrefix]
    internal static bool ShowRole(IntroCutscene __instance)
    {
        if (!Helpers.RolesEnabled) return true;
        seed = Main.Random.Next(5000);
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
        {
            SetRoleTexts(__instance);
        })));
        return true;
    }

    private static void SetRoleTexts(IntroCutscene __instance)
    {
        List<RoleInfo> roleInfos = RoleInfoList.GetRoleInfoForPlayer(CachedPlayer.LocalPlayer.PlayerControl);
        List<ModifierInfo> modifierInfos = ModifierInfoList.GetModifierInfoForPlayer(CachedPlayer.LocalPlayer.PlayerControl);
        RoleInfo roleInfo = roleInfos.FirstOrDefault();
        ModifierInfo modifierInfo = modifierInfos.FirstOrDefault();

        __instance.RoleBlurbText.text = "";
        if (roleInfo != null)
        {
            __instance.YouAreText.color = roleInfo.RoleColor;
            __instance.RoleText.text = roleInfo.Name;
            __instance.RoleText.color = roleInfo.RoleColor;
            __instance.RoleBlurbText.text = roleInfo.IntroDescription;
            __instance.RoleBlurbText.color = roleInfo.RoleColor;
        }

        if (PlayerControl.LocalPlayer.IsRole(RoleType.Madmate))
        {
            __instance.YouAreText.color = ImpostorRed;
            __instance.RoleText.text = ModResources.Madmate;
            __instance.RoleText.color = ImpostorRed;
            __instance.RoleBlurbText.text = ModResources.MadmateIntro;
            __instance.RoleBlurbText.color = ImpostorRed;
        }
        if (modifierInfo != null)
        {
            __instance.RoleBlurbText.text += $"\n{modifierInfo.IntroDescription}";
        }
    }

    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate)), HarmonyPrefix]
    internal static void BeginCrewmatePrefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        SetupIntroTeamIcons(__instance, ref teamToDisplay);
    }


    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate)), HarmonyPostfix]
    internal static void BeginCrewmatePostfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        SetupIntroTeam(__instance, ref teamToDisplay);
    }

    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor)), HarmonyPrefix]
    internal static void BeginImpostorPrefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        SetupIntroTeamIcons(__instance, ref yourTeam);
    }

    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor)), HarmonyPostfix]
    internal static void BeginImpostorPostfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        SetupIntroTeam(__instance, ref yourTeam);
    }
}