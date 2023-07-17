namespace AmongDogUs.Roles;

internal class RoleInfo
{
    internal Color RoleColor;
    internal virtual string Name { get { return AmongDogUs.GetString(NameKey); } }
    internal virtual string ColorName { get { return Helpers.ColorString(RoleColor, Name); } }
    internal virtual string IntroDescription { get { return AmongDogUs.GetString($"{NameKey}Intro"); } }
    internal virtual string ShortDescription { get { return AmongDogUs.GetString($"{NameKey}Short"); } }
    internal virtual string FullDescription { get { return AmongDogUs.GetString($"{NameKey}Full"); } }
    internal virtual string RoleOptions { get { return GameOptionsDataPatch.OptionsToString(BaseOption, true); } }

    internal RoleId RoleId;
    internal string NameKey;
    internal CustomOption BaseOption;

    internal RoleInfo(string Name, Color RoleColor, CustomOption BaseOption, RoleId RoleId)
    {
        this.RoleColor = RoleColor;
        NameKey = Name;
        this.BaseOption = BaseOption;
        this.RoleId = RoleId;
    }
}

internal static class RoleInfoList
{
    internal static List<RoleInfo> AllRoleInfos; // RoleInfoを全て格納するList

    internal static RoleInfo Impostor;
    internal static RoleInfo Crewmate;

    internal static void Load()
    {
        AllRoleInfos = new()
        {
            Impostor,
            Crewmate,
        };

        Impostor = new("Impostor", Palette.ImpostorRed, null, RoleId.Impostor);
        Crewmate = new("Crewmate", Palette.CrewmateBlue, null, RoleId.Crewmate);
    }

    internal static List<RoleInfo> GetRoleInfoForPlayer(PlayerControl p, RoleId[] excludeRoles = null, bool includeHidden = false)
    {
        List<RoleInfo> infos = new();
        if (p == null) return infos;

        if (infos.Count == 0 && p.Data.Role != null && p.Data.Role.IsImpostor) infos.Add(Impostor); // Just Impostor
        if (infos.Count == 0 && p.Data.Role != null && !p.Data.Role.IsImpostor) infos.Add(Crewmate); // Just Crewmate

        if (excludeRoles != null) infos.RemoveAll(x => excludeRoles.Contains(x.RoleId));

        return infos;
    }

    internal static string GetRolesString(PlayerControl p, bool useColors, RoleId[] excludeRoles = null, bool includeHidden = false)
    {
        if (p?.Data?.Disconnected != false) return "";

        var roleInfo = GetRoleInfoForPlayer(p, excludeRoles, includeHidden);
        string roleName = string.Join(" ", roleInfo.Select(x => Helpers.ColorString(x.RoleColor, x.Name)).ToArray());

        // if (p.HasModifier(ModifierId.Opportunist))
        // {
        //     string postfix = Helpers.cs(OpportunistGreen, p.GetModifierPostfixString(ModifierId.Opportunist));
        //     roleName += postfix;
        // }

        return roleName;
    }
}