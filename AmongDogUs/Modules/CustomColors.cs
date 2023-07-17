// Source Code from TheOtherRoles (https://github.com/TheOtherRolesAU/TheOtherRoles)

namespace AmongDogUs.Modules;

internal class CustomColors
{
    protected static Dictionary<int, string> ColorStrings = new();
    internal static readonly List<int> lighterColors = new() { 3, 4, 5, 7, 10, 11, 13, 14, 17 };
    internal static uint pickableColors = (uint)Palette.ColorNames.Length;

    private static readonly int[] ORDER = new[] { 7, 14, 5, 33, 4,
                                                    30, 0, 19, 27, 3,
                                                    17, 25, 18, 13, 23,
                                                    8, 32, 1, 21, 31,
                                                    10, 34, 15, 28, 22,
                                                    29, 11, 2, 26, 16,
                                                    20, 24, 9, 12, 6
                                                };
    internal static void Load()
    {
        List<StringNames> longList = Enumerable.ToList(Palette.ColorNames);
        List<Color32> colorList = Enumerable.ToList(Palette.PlayerColors);
        List<Color32> shadowList = Enumerable.ToList(Palette.ShadowColors);

        List<CustomColor> colors = new()
        {
            /* Custom Colors */
            new()
            {
                longName = ModResources.ColorSalmon,
                color = new Color32(239, 191, 192, byte.MaxValue), // color = new Color32(0xD8, 0x82, 0x83, byte.MaxValue),
                shadow = new Color32(182, 119, 114, byte.MaxValue), // shadow = new Color32(0xA5, 0x63, 0x65, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorBordeaux,
                color = new Color32(109, 7, 26, byte.MaxValue),
                shadow = new Color32(54, 2, 11, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorOlive,
                color = new Color32(154, 140, 61, byte.MaxValue),
                shadow = new Color32(104, 95, 40, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorTurqoise,
                color = new Color32(22, 132, 176, byte.MaxValue),
                shadow = new Color32(15, 89, 117, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorMint,
                color = new Color32(111, 192, 156, byte.MaxValue),
                shadow = new Color32(65, 148, 111, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorLavender,
                color = new Color32(173, 126, 201, byte.MaxValue),
                shadow = new Color32(131, 58, 203, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorNougat,
                color = new Color32(160, 101, 56, byte.MaxValue),
                shadow = new Color32(115, 15, 78, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorPeach,
                color = new Color32(255, 164, 119, byte.MaxValue),
                shadow = new Color32(238, 128, 100, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorWasabi,
                color = new Color32(112, 143, 46, byte.MaxValue),
                shadow = new Color32(72, 92, 29, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorHotPink,
                color = new Color32(255, 51, 102, byte.MaxValue),
                shadow = new Color32(232, 0, 58, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorPetrol,
                color = new Color32(0, 99, 105, byte.MaxValue),
                shadow = new Color32(0, 61, 54, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorLemon,
                color = new Color32(0xDB, 0xFD, 0x2F, byte.MaxValue),
                shadow = new Color32(0x74, 0xE5, 0x10, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorSignalOrange,
                color = new Color32(0xF7, 0x44, 0x17, byte.MaxValue),
                shadow = new Color32(0x9B, 0x2E, 0x0F, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorTeal,
                color = new Color32(0x25, 0xB8, 0xBF, byte.MaxValue),
                shadow = new Color32(0x12, 0x89, 0x86, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorBlurple,
                color = new Color32(0x59, 0x3C, 0xD6, byte.MaxValue),
                shadow = new Color32(0x29, 0x17, 0x96, byte.MaxValue),
                isLighterColor = false
            },
            new()
            {
                longName = ModResources.ColorSunrise,
                color = new Color32(0xFF, 0xCA, 0x19, byte.MaxValue),
                shadow = new Color32(0xDB, 0x44, 0x42, byte.MaxValue),
                isLighterColor = true
            },
            new()
            {
                longName = ModResources.ColorIce,
                color = new Color32(0xA8, 0xDF, 0xFF, byte.MaxValue),
                shadow = new Color32(0x59, 0x9F, 0xC8, byte.MaxValue),
                isLighterColor = true
            }
        };

        pickableColors += (uint)colors.Count; // Colors to show in Tab
        /** Hidden Colors **/

        /** Add Colors **/
        int id = 50000;
        foreach (CustomColor cc in colors)
        {
            longList.Add((StringNames)id);
            ColorStrings[id++] = cc.longName;
            colorList.Add(cc.color);
            shadowList.Add(cc.shadow);
            if (cc.isLighterColor) lighterColors.Add(colorList.Count - 1);
        }

        Palette.ColorNames = longList.ToArray();
        Palette.PlayerColors = colorList.ToArray();
        Palette.ShadowColors = shadowList.ToArray();
    }

    protected internal struct CustomColor
    {
        internal string longName;
        internal Color32 color;
        internal Color32 shadow;
        internal bool isLighterColor;
    }

    [HarmonyPatch]
    internal static class CustomColorPatches
    {
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]

        private class ColorStringPatch
        {
            [HarmonyPriority(Priority.Last)]
            internal static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
            {
                if ((int)name >= 50000)
                {
                    string text = ColorStrings[(int)name];
                    if (text != null)
                    {
                        __result = text;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
        private static class PlayerTabEnablePatch
        {
            internal static void Postfix(PlayerTab __instance)
            {
                // Replace instead
                Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                int cols = 5;
                for (int i = 0; i < ORDER.Length; i++)
                {
                    int pos = ORDER[i];
                    if (pos < 0 || pos > chips.Length) continue;
                    ColorChip chip = chips[pos];
                    int row = i / cols, col = i % cols; // Dynamically do the positioning
                    chip.transform.localPosition = new Vector3(-0.975f + (col * 0.485f), 1.475f - (row * 0.49f), chip.transform.localPosition.z);
                    chip.transform.localScale *= 0.78f;
                }
                for (int j = ORDER.Length; j < chips.Length; j++)
                {
                    // If number isn't in order, hide it
                    ColorChip chip = chips[j];
                    chip.transform.localScale *= 0f;
                    chip.enabled = false;
                    chip.Button.enabled = false;
                    chip.Button.OnClick.RemoveAllListeners();
                }
            }
        }

        [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
        private static class LoadPlayerPrefsPatch
        {
            // Fix Potential issues with broken colors
            private static bool needsPatch = false;
            internal static void Prefix([HarmonyArgument(0)] bool overrideLoad)
            {
                if (!LegacySaveManager.loaded || overrideLoad) needsPatch = true;
            }

            internal static void Postfix()
            {
                if (!needsPatch) return;
                LegacySaveManager.colorConfig %= pickableColors;
                needsPatch = false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
        private static class PlayerControlCheckColorPatch
        {
            private static bool IsTaken(PlayerControl player, uint color)
            {
                foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers.GetFastEnumerator())
                    if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color) return true;
                return false;
            }

            internal static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
            {
                // Fix incorrect color assignment
                uint color = bodyColor;
                if (IsTaken(__instance, color) || color >= Palette.PlayerColors.Length)
                {
                    int num = 0;
                    while (num++ < 50 && (color >= pickableColors || IsTaken(__instance, color)))
                    {
                        color = (color + 1) % pickableColors;
                    }
                }
                __instance.RpcSetColor((byte)color);
                return false;
            }
        }
    }
}