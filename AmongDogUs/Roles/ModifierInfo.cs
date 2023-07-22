namespace AmongDogUs.Roles;

internal class ModifierInfo
{
    internal Color RoleColor;
    internal virtual string Name { get { return AmongDogUs.GetString(NameKey); } }
    internal virtual string ColorName { get { return Helpers.ColorString(RoleColor, Name); } }
    internal virtual string IntroDescription { get { return AmongDogUs.GetString($"{NameKey}Intro"); } }
    internal virtual string ShortDescription { get { return AmongDogUs.GetString($"{NameKey}Short"); } }
    internal virtual string FullDescription { get { return AmongDogUs.GetString($"{NameKey}Full"); } }
    internal virtual string RoleOptions { get { return GameOptionsDataPatch.OptionsToString(BaseOption, true); } }

    internal ModifierType ModId;
    internal string NameKey;
    internal CustomOption BaseOption;

    internal ModifierInfo(string Name, Color RoleColor, CustomOption BaseOption, ModifierType ModId)
    {
        this.RoleColor = RoleColor;
        NameKey = Name;
        this.BaseOption = BaseOption;
        this.ModId = ModId;
    }
}

internal static class ModifierInfoList
{
    internal static List<ModifierInfo> AllRoleInfos;

    internal static ModifierInfo Opportunist;
    internal static ModifierInfo Sunglasses;
    internal static ModifierInfo Watcher;

    internal static void Load()
    {
        AllRoleInfos = new()
        {
            Opportunist,
            Sunglasses,
            Watcher,
        };

        Opportunist = new("Opportunist", Palette.ImpostorRed, null, ModifierType.Opportunist);
        Sunglasses = new("Sunglasses", Palette.ImpostorRed, null, ModifierType.Sunglasses);
        Watcher = new("Watcher", Palette.ImpostorRed, null, ModifierType.Watcher);
    }

    internal static List<ModifierInfo> GetModifierInfoForPlayer(PlayerControl p, ModifierType[] excludeRoles = null, bool _ = false)
    {
        List<ModifierInfo> infos = new();
        if (p == null) return infos;

        if (excludeRoles != null) infos.RemoveAll(x => excludeRoles.Contains(x.ModId));

        return infos;
    }
}