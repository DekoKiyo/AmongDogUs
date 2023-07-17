namespace AmongDogUs;

internal class DeadPlayer
{
    internal PlayerControl player;
    internal DateTime timeOfDeath;
    internal DeathReason deathReason;
    internal PlayerControl killerIfExisting;

    internal DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason, PlayerControl killerIfExisting)
    {
        this.player = player;
        this.timeOfDeath = timeOfDeath;
        this.deathReason = deathReason;
        this.killerIfExisting = killerIfExisting;
    }
}

internal static class GameHistory
{
    internal static List<Tuple<Vector3, bool>> localPlayerPositions = new();
    internal static List<DeadPlayer> deadPlayers = new();
    internal static Dictionary<int, EFinalStatus> finalStatuses = new();

    internal static void ClearGameHistory()
    {
        localPlayerPositions = new List<Tuple<Vector3, bool>>();
        deadPlayers = new List<DeadPlayer>();
        finalStatuses = new Dictionary<int, EFinalStatus>();
    }
}