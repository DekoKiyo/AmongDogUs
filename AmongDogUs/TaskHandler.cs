namespace AmongDogUs;

[HarmonyPatch]
internal static class TasksHandler
{
    internal static Tuple<int, int> TaskInfo(GameData.PlayerInfo playerInfo)
    {
        int TotalTasks = 0;
        int CompletedTasks = 0;
        if (!playerInfo.Disconnected && playerInfo.Tasks != null &&
            playerInfo.Object &&
            playerInfo.Role && playerInfo.Role.TasksCountTowardProgress &&
            !playerInfo.Object.HasFakeTasks()
            )
        {
            for (int j = 0; j < playerInfo.Tasks.Count; j++)
            {
                TotalTasks++;
                if (playerInfo.Tasks[j].Complete)
                {
                    CompletedTasks++;
                }
            }
        }
        return Tuple.Create(CompletedTasks, TotalTasks);
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
    private static class GameDataRecomputeTaskCountsPatch
    {
        private static bool Prefix(GameData __instance)
        {
            var totalTasks = 0;
            var completedTasks = 0;
            for (int i = 0; i < __instance.AllPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                if (playerInfo.Object && (Madmate.HasTasks && playerInfo.Object.IsRole(RoleType.Madmate))) continue;
                var (playerCompleted, playerTotal) = TaskInfo(playerInfo);
                totalTasks += playerTotal;
                completedTasks += playerCompleted;
            }
            __instance.TotalTasks = totalTasks;
            __instance.CompletedTasks = completedTasks;
            return false;
        }
    }
}