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

    internal RoleType RoleId;
    internal string NameKey;
    internal CustomOption BaseOption;

    internal RoleInfo(string Name, Color RoleColor, CustomOption BaseOption, RoleType RoleId)
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
    internal static RoleInfo Sheriff;
    internal static RoleInfo ProEngineer;
    internal static RoleInfo Madmate;
    internal static RoleInfo Bakery;
    internal static RoleInfo Snitch;
    internal static RoleInfo Seer;
    internal static RoleInfo Lighter;
    internal static RoleInfo Altruist;
    internal static RoleInfo Mayor;
    internal static RoleInfo CustomImpostor;
    internal static RoleInfo UnderTaker;
    internal static RoleInfo BountyHunter;
    internal static RoleInfo Teleporter;
    internal static RoleInfo EvilHacker;
    internal static RoleInfo Jester;
    internal static RoleInfo Jackal;
    internal static RoleInfo Sidekick;
    internal static RoleInfo Arsonist;

    internal static void Load()
    {
        AllRoleInfos = new()
        {
            Impostor,
            Crewmate,
            Sheriff,
            ProEngineer,
            Madmate,
            Bakery,
            Snitch,
            Seer,
            Lighter,
            Altruist,
            Mayor,
            CustomImpostor,
            UnderTaker,
            BountyHunter,
            Teleporter,
            EvilHacker,
            Jester,
            Jackal,
            Sidekick,
            Arsonist,
        };

        Impostor = new("Impostor", ImpostorRed, null, RoleType.Impostor);
        Crewmate = new("Crewmate", CrewmateBlue, null, RoleType.Crewmate);
        Sheriff = new("Sheriff", SheriffYellow, CustomOptionsHolder.SheriffRate, RoleType.Sheriff);
        ProEngineer = new("ProEngineer", EngineerBlue, CustomOptionsHolder.EngineerRate, RoleType.ProEngineer);
        Madmate = new("Madmate", ImpostorRed, CustomOptionsHolder.MadmateRate, RoleType.Madmate);
        Bakery = new("Bakery", BakeryYellow, CustomOptionsHolder.BakeryRate, RoleType.Bakery);
        Snitch = new("Snitch", SnitchGreen, CustomOptionsHolder.SnitchRate, RoleType.Snitch);
        Seer = new("Seer", SeerGreen, CustomOptionsHolder.SeerRate, RoleType.Seer);
        Lighter = new("Lighter", LighterYellow, CustomOptionsHolder.LighterRate, RoleType.Lighter);
        Altruist = new("Altruist", AltruistRed, CustomOptionsHolder.AltruistRate, RoleType.Altruist);
        Mayor = new("Mayor", MayorGreen, CustomOptionsHolder.MayorRate, RoleType.Mayor);
        CustomImpostor = new("CustomImpostor", ImpostorRed, CustomOptionsHolder.CustomImpostorRate, RoleType.CustomImpostor);
        UnderTaker = new("UnderTaker", ImpostorRed, CustomOptionsHolder.UnderTakerRate, RoleType.UnderTaker);
        BountyHunter = new("BountyHunter", ImpostorRed, CustomOptionsHolder.BountyHunterRate, RoleType.BountyHunter);
        Teleporter = new("Teleporter", ImpostorRed, CustomOptionsHolder.TeleporterRate, RoleType.Teleporter);
        EvilHacker = new("EvilHacker", ImpostorRed, CustomOptionsHolder.EvilHackerRate, RoleType.EvilHacker);
        Jester = new("Jester", JesterPink, CustomOptionsHolder.JesterRate, RoleType.Jester);
        Jackal = new("Jackal", JackalBlue, CustomOptionsHolder.JackalRate, RoleType.Jackal);
        Sidekick = new("Sidekick", JackalBlue, CustomOptionsHolder.JackalRate, RoleType.Sidekick);
        Arsonist = new("Arsonist", ArsonistOrange, CustomOptionsHolder.ArsonistRate, RoleType.Arsonist);
    }

    internal static List<RoleInfo> GetRoleInfoForPlayer(PlayerControl p, RoleType[] excludeRoles = null, bool _ = false)
    {
        List<RoleInfo> infos = new();
        if (p == null) return infos;

        if (p.IsRole(RoleType.Sheriff)) infos.Add(Sheriff);
        if (p.IsRole(RoleType.ProEngineer)) infos.Add(ProEngineer);
        if (p.IsRole(RoleType.Madmate)) infos.Add(Madmate);
        if (p.IsRole(RoleType.Bakery)) infos.Add(Bakery);
        if (p.IsRole(RoleType.Snitch)) infos.Add(Snitch);
        if (p.IsRole(RoleType.Seer)) infos.Add(Seer);
        if (p.IsRole(RoleType.Lighter)) infos.Add(Lighter);
        if (p.IsRole(RoleType.Altruist)) infos.Add(Altruist);
        if (p.IsRole(RoleType.Mayor)) infos.Add(Mayor);
        if (p.IsRole(RoleType.CustomImpostor)) infos.Add(CustomImpostor);
        if (p.IsRole(RoleType.UnderTaker)) infos.Add(UnderTaker);
        if (p.IsRole(RoleType.BountyHunter)) infos.Add(BountyHunter);
        if (p.IsRole(RoleType.Teleporter)) infos.Add(Teleporter);
        if (p.IsRole(RoleType.EvilHacker)) infos.Add(EvilHacker);
        if (p.IsRole(RoleType.Jester)) infos.Add(Jester);
        if (p.IsRole(RoleType.Jackal)) infos.Add(Jackal);
        if (p.IsRole(RoleType.Sidekick)) infos.Add(Sidekick);
        if (p.IsRole(RoleType.Arsonist)) infos.Add(Arsonist);

        if (infos.Count == 0 && p.Data.Role != null && p.Data.Role.IsImpostor) infos.Add(Impostor); // Just Impostor
        if (infos.Count == 0 && p.Data.Role != null && !p.Data.Role.IsImpostor) infos.Add(Crewmate); // Just Crewmate

        if (excludeRoles != null) infos.RemoveAll(x => excludeRoles.Contains(x.RoleId));

        return infos;
    }

    internal static string GetRolesString(PlayerControl p, bool _, RoleType[] excludeRoles = null, bool includeHidden = false)
    {
        if (p?.Data?.Disconnected != false) return "";

        var roleInfo = GetRoleInfoForPlayer(p, excludeRoles, includeHidden);
        string roleName = string.Join(" ", roleInfo.Select(x => Helpers.ColorString(x.RoleColor, x.Name)).ToArray());

        if (p.HasModifier(ModifierType.Opportunist)) roleName += Helpers.ColorString(OpportunistGreen, p.GetModifierPostfixString(ModifierType.Opportunist));
        if (p.HasModifier(ModifierType.Watcher)) roleName += Helpers.ColorString(WatcherPurple, p.GetModifierPostfixString(ModifierType.Watcher));
        if (p.HasModifier(ModifierType.Sunglasses)) roleName += Helpers.ColorString(SunglassesGray, p.GetModifierPostfixString(ModifierType.Watcher));

        return roleName;
    }
}