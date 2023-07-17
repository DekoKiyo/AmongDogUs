namespace AmongDogUs;

internal static class Helpers
{
    internal static bool ShowButtons
    {
        get
        {
            return !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                    !MeetingHud.Instance &&
                    !ExileController.Instance &&
                    GameStarted;
        }
    }
    internal static bool ShowMeetingText
    {
        get
        {
            return MeetingHud.Instance != null &&
                (MeetingHud.Instance.state == MeetingHud.VoteStates.Voted ||
                MeetingHud.Instance.state == MeetingHud.VoteStates.NotVoted ||
                MeetingHud.Instance.state == MeetingHud.VoteStates.Discussion);
        }
    }
    internal static bool GameStarted { get { return AmongUsClient.Instance != null && AmongUsClient.Instance.GameState is InnerNetClient.GameStates.Started; } }
    internal static bool RolesEnabled { get { return CustomOptionsHolder.ActivateModRoles.GetBool(); } }
    internal static bool RefundVotes { get { return CustomOptionsHolder.RefundVotesOnDeath.GetBool(); } }
    internal static bool IsMirrorMap { get { return CustomOptionsHolder.EnableMirrorMap.GetBool(); } }

    internal static Dictionary<string, Sprite> CachedSprites = new();

    public static Sprite LoadSpriteFromTexture2D(Texture2D texture, float pixelsPerUnit)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    internal static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            System.Console.WriteLine("Error loading sprite from path: " + path);
        }
        return null;
    }

    internal static unsafe Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var byteTexture = new byte[stream.Length];
            _ = stream.Read(byteTexture, 0, (int)stream.Length);
            LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from resources: " + path);
        }
        return null;
    }

    internal static Texture2D LoadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                byte[] byteTexture = File.ReadAllBytes(path);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
        }
        catch
        {
            System.Console.WriteLine("Error loading texture from disk: " + path);
        }
        return null;
    }

    internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
    internal static d_LoadImage iCall_LoadImage;
    private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
    {
        iCall_LoadImage ??= IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        var il2cppArray = (Il2CppStructArray<byte>)data;
        return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
    }

    internal static string ColorString(Color c, string s)
    {
        return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }

    internal static bool IsDead(this PlayerControl player)
    {
        return player == null || player?.Data?.IsDead == true || player?.Data?.Disconnected == true ||
            (finalStatuses != null && finalStatuses.ContainsKey(player.PlayerId) && finalStatuses[player.PlayerId] != EFinalStatus.Alive);
    }

    internal static bool IsAlive(this PlayerControl player)
    {
        return !IsDead(player);
    }

    internal static List<byte> GenerateTasks(int NumCommon, int NumShort, int NumLong)
    {
        if (NumCommon + NumShort + NumLong <= 0) NumShort = 1;

        var tasks = new Il2CppSystem.Collections.Generic.List<byte>();
        var hashSet = new Il2CppSystem.Collections.Generic.HashSet<TaskTypes>();

        var commonTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
        foreach (var task in ShipStatus.Instance.CommonTasks.OrderBy(x => Main.Random.Next())) commonTasks.Add(task);

        var shortTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
        foreach (var task in ShipStatus.Instance.NormalTasks.OrderBy(x => Main.Random.Next())) shortTasks.Add(task);

        var longTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
        foreach (var task in ShipStatus.Instance.LongTasks.OrderBy(x => Main.Random.Next())) longTasks.Add(task);

        int start = 0;
        ShipStatus.Instance.AddTasksFromList(ref start, NumCommon, tasks, hashSet, commonTasks);

        start = 0;
        ShipStatus.Instance.AddTasksFromList(ref start, NumShort, tasks, hashSet, shortTasks);

        start = 0;
        ShipStatus.Instance.AddTasksFromList(ref start, NumLong, tasks, hashSet, longTasks);

        return tasks.ToArray().ToList();
    }

    internal static void ShareGameVersion()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.VersionHandshake, SendOption.Reliable, -1);
        writer.WritePacked(Main.PLUGIN_VERSION_MAJOR);
        writer.WritePacked(Main.PLUGIN_VERSION_MINOR);
        writer.WritePacked(Main.PLUGIN_VERSION_BUILD);
        writer.WritePacked(AmongUsClient.Instance.ClientId);
        writer.Write((byte)(Main.PLUGIN_VERSION_REVISION < 0 ? 0xFF : Main.PLUGIN_VERSION_REVISION));
        writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.VersionHandshake(Main.PLUGIN_VERSION_MAJOR, Main.PLUGIN_VERSION_MINOR, Main.PLUGIN_VERSION_BUILD, Main.PLUGIN_VERSION_REVISION, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
    }

    internal static void RefreshRoleDescription(PlayerControl player)
    {
        if (player == null) return;

        List<RoleInfo> infos = RoleInfoList.GetRoleInfoForPlayer(player);

        var toRemove = new List<PlayerTask>();
        foreach (PlayerTask t in player.myTasks.GetFastEnumerator())
        {
            var textTask = t.gameObject.GetComponent<ImportantTextTask>();
            if (textTask != null)
            {
                var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(x.Name));
                if (info != null) infos.Remove(info); // TextTask for this RoleInfo does not have to be added, as it already exists
                else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
            }
        }

        foreach (PlayerTask t in toRemove)
        {
            t.OnRemove();
            player.myTasks.Remove(t);
            Object.Destroy(t.gameObject);
        }

        // Add TextTask for remaining RoleInfos
        foreach (RoleInfo roleInfo in infos)
        {
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);

            // if (roleInfo.RoleId is RoleId.Jackal && Jackal.CanSidekick)
            // {
            //     task.Text += cs(roleInfo.RoleColor, LocalizationManager.GetString(TransKey.JackalWithSidekick));
            // }
            // else
            // {
            task.Text = ColorString(roleInfo.RoleColor, $"{roleInfo.Name}: {roleInfo.ShortDescription}");
            // }

            player.myTasks.Insert(0, task);
        }
    }

    internal static PlayerControl PlayerById(byte id)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            if (player.PlayerId == id) return player;
        return null;
    }

    internal static void ClearAllTasks(this PlayerControl player)
    {
        if (player is null) return;
        for (int i = 0; i < player.myTasks.Count; i++)
        {
            PlayerTask playerTask = player.myTasks[i];
            playerTask.OnRemove();
            Object.Destroy(playerTask.gameObject);
        }
        player.myTasks.Clear();

        if (player.Data != null && player.Data.Tasks != null) player.Data.Tasks.Clear();
    }

    internal static bool IsCrew(this PlayerControl player)
    {
        return player != null && !player.IsImpostor() && !player.IsNeutral();
    }

    internal static bool IsImpostor(this PlayerControl player)
    {
        return player != null && player.Data.Role.IsImpostor;
    }

    internal static bool HasFakeTasks(this PlayerControl player)
    {
        return (player.IsNeutral() && !player.NeutralHasTasks()) /*|| (player.IsRole(RoleId.Madmate) && !Madmate.HasTasks)*/;
    }

    internal static bool NeutralHasTasks(this PlayerControl player)
    {
        return player.IsNeutral() /*&& (player.IsRole(RoleId.Jester) && Jester.HasTasks)*/;
    }

    internal static bool IsNeutral(this PlayerControl player)
    {
        return player != null &&/*
                (player.IsRole(RoleId.Jester) ||
                player.IsRole(RoleId.Arsonist) ||*/
                player.IsTeamJackal();
    }

    public static bool IsTeamJackal(this PlayerControl player)
    {
        return (player != null/* &&
                    (player.IsRole(RoleId.Jackal) ||
                    player.IsRole(RoleId.Sidekick))*/);
    }
}