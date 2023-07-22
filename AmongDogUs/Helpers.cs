namespace AmongDogUs;

internal enum MurderAttemptResult
{
    PerformKill,
    SuppressKill,
    BlankKill
}

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

    internal static Sprite LoadSpriteFromTexture2D(Texture2D texture, float pixelsPerUnit)
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
        List<RoleInfo> infos = RoleInfoList.GetRoleInfoForPlayer(player);
        List<string> taskTexts = new(infos.Count);

        foreach (var roleInfo in infos)
        {
            taskTexts.Add(GetRoleString(roleInfo));
        }

        var toRemove = new List<PlayerTask>();
        foreach (PlayerTask t in player.myTasks.GetFastEnumerator())
        {
            var textTask = t.TryCast<ImportantTextTask>();
            if (textTask == null) continue;

            var currentText = textTask.Text;

            if (taskTexts.Contains(currentText)) taskTexts.Remove(currentText); // TextTask for this RoleInfo does not have to be added, as it already exists
            else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
        }

        foreach (PlayerTask t in toRemove)
        {
            t.OnRemove();
            player.myTasks.Remove(t);
            Object.Destroy(t.gameObject);
        }

        // Add TextTask for remaining RoleInfos
        foreach (string title in taskTexts)
        {
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);
            task.Text = title;
            player.myTasks.Insert(0, task);
        }
    }

    internal static string GetRoleString(RoleInfo roleInfo)
    {
        if (roleInfo.RoleId is RoleType.Jackal && Jackal.CanSidekick)
            return ColorString(roleInfo.RoleColor, $"{roleInfo.Name}: {ModResources.JackalWithSidekick}");
        return ColorString(roleInfo.RoleColor, $"{roleInfo.Name}: {roleInfo.ShortDescription}");
    }

    internal static PlayerControl PlayerById(byte id)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls) if (player.PlayerId == id) return player;
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
        return (player.IsNeutral() && !player.NeutralHasTasks()) || (player.IsRole(RoleType.Madmate) && !Madmate.HasTasks);
    }

    internal static bool NeutralHasTasks(this PlayerControl player)
    {
        return player.IsNeutral() & (player.IsRole(RoleType.Jester) && Jester.HasTasks);
    }

    internal static bool IsNeutral(this PlayerControl player)
    {
        return player != null &&
                (player.IsRole(RoleType.Jester) ||
                player.IsRole(RoleType.Arsonist) ||
                player.IsTeamJackal());
    }

    internal static bool IsTeamJackal(this PlayerControl player)
    {
        return player != null &&
                (player.IsRole(RoleType.Jackal) ||
                player.IsRole(RoleType.Sidekick));
    }

    internal static byte GetMapId()
    {
        return GameOptionsManager.Instance.currentGameOptions.GetByte(ByteOptionNames.MapId);
    }

    internal static void GenerateAndAssignTasks(this PlayerControl player, int NumCommon, int NumShort, int NumLong)
    {
        if (player == null) return;

        List<byte> taskTypeIds = GenerateTasks(NumCommon, NumShort, NumLong);

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UncheckedSetTasks, SendOption.Reliable, -1);
        writer.Write(player.PlayerId);
        writer.WriteBytesAndSize(taskTypeIds.ToArray());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.UncheckedSetTasks(player.PlayerId, taskTypeIds.ToArray());
    }

    internal static MurderAttemptResult CheckMurderAttempt(PlayerControl killer, PlayerControl target)
    {
        // Modified vanilla checks
        if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
        if (killer == null || killer.Data == null || killer.Data.IsDead || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
        if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code

        return MurderAttemptResult.PerformKill;
    }

    internal static MurderAttemptResult CheckMurderAttemptAndKill(PlayerControl killer, PlayerControl target, bool showAnimation = true)
    {
        // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
        // The kill attempt will be shared using a custom RPC, hence combining modded and unModded versions is impossible

        MurderAttemptResult murder = CheckMurderAttempt(killer, target);
        if (murder == MurderAttemptResult.PerformKill)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)ECustomRPC.UncheckedMurderPlayer, SendOption.Reliable, -1);
            writer.Write(killer.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(showAnimation ? byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.UncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? byte.MaxValue : (byte)0);
        }
        return murder;
    }

    internal static T GetRandom<T>(this List<T> list)
    {
        var random = Main.Random.Next(0, list.Count);
        return list[random];
    }

    internal static void SetSemiTransparent(this PoolablePlayer player, bool value)
    {
        float alpha = value ? 0.25f : 1f;
        foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
            r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
        player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r, player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
    }

    internal static bool HidePlayerName(PlayerControl target)
    {
        return HidePlayerName(PlayerControl.LocalPlayer, target);
    }

    internal static bool HidePlayerName(PlayerControl source, PlayerControl target)
    {
        if (source == target) return false;
        if (source == null || target == null) return true;
        if (source.IsDead()) return false;
        if (target.IsDead()) return true;
        else if (source.Data.Role.IsImpostor && target.Data.Role.IsImpostor) return false;/* // Members of team Impostors see the names of Impostors/Spies
            // if (Camouflager.camouflageTimer > 0f) return true; // No names are visible
            // if (!source.isImpostor() && Ninja.isStealthed(target)) return true; // Hide ninja nametags from non-impostors
            // if (!source.IsRole(RoleId.Fox) && !source.Data.IsDead && Fox.isStealthed(target)) return true;
            */
        if (ModMapOptions.HideOutOfSightNametags && GameStarted && ShipStatus.Instance != null && source.transform != null && target.transform != null)
        {
            float distMod = 1.025f;
            float distance = Vector3.Distance(source.transform.position, target.transform.position);
            bool anythingBetween = PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShadowMask, false);

            if (distance > ShipStatus.Instance.CalculateLightRadius(source.Data) * distMod || anythingBetween) return true;
        }
        if (!ModMapOptions.HidePlayerNames) return false; // All names are visible
        // if (source.isImpostor() && (target.isImpostor() || target.IsRole(RoleId.Spy))) return false; // Members of team Impostors see the names of Impostors/Spies
        // if (source.getPartner() == target) return false; // Members of team Lovers see the names of each other
        if ((source.IsRole(RoleType.Jackal) || source.IsRole(RoleType.Sidekick)) && (target.IsRole(RoleType.Jackal) || target.IsRole(RoleType.Sidekick))) return false; // Members of team Jackal see the names of each other
        return true;
    }

    internal static void ShowFlash(Color color, float duration = 1f)
    {
        if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
        FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
        {
            var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

            if (p < 0.5)
            {
                if (renderer != null) renderer.color = new(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
            }
            else
            {
                if (renderer != null) renderer.color = new(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
            }
            if (p == 1f && renderer != null) renderer.enabled = false;
        })));
    }

    internal static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
    {
        tie = true;
        KeyValuePair<byte, int> result = new(byte.MaxValue, int.MinValue);
        foreach (KeyValuePair<byte, int> keyValuePair in self)
        {
            if (keyValuePair.Value > result.Value)
            {
                result = keyValuePair;
                tie = false;
            }
            else if (keyValuePair.Value == result.Value)
            {
                tie = true;
            }
        }
        return result;
    }

    internal static bool RoleCanUseVents(this PlayerControl player)
    {
        bool roleCouldUse = false;
        if (ProEngineer.CanUseVents && player.IsRole(RoleType.ProEngineer)) roleCouldUse = true;
        else if (Jester.CanUseVents && player.IsRole(RoleType.Jester)) roleCouldUse = true;
        else if (Madmate.CanUseVents && player.IsRole(RoleType.Madmate)) roleCouldUse = true;
        else if (Jackal.CanUseVents && player.IsRole(RoleType.Jackal)) roleCouldUse = true;
        else if (Sidekick.CanUseVents && player.IsRole(RoleType.Sidekick)) roleCouldUse = true;
        else if (player.Data?.Role != null && player.Data.Role.CanVent)
        {
            if (!CustomImpostor.CanUseVents && player.IsRole(RoleType.CustomImpostor)) roleCouldUse = false;
            else roleCouldUse = true;
        }
        return roleCouldUse;
    }

    internal static bool RoleCanSabotage(this PlayerControl player)
    {
        bool roleCouldUse = false;
        if (Jester.CanSabotage && player.IsRole(RoleType.Jester)) roleCouldUse = true;
        else if (Madmate.CanSabotage && player.IsRole(RoleType.Madmate)) roleCouldUse = true;
        else if (!CustomImpostor.CanSabotage && player.IsRole(RoleType.CustomImpostor)) roleCouldUse = false;
        else if (player.Data?.Role != null && player.Data.Role.IsImpostor) roleCouldUse = true;

        return roleCouldUse;
    }

    internal static object TryCast(this Il2CppObjectBase self, Type type)
    {
        return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
    }

    internal static DeadBody DeadBodyById(byte id)
    {
        foreach (DeadBody deadBody in Object.FindObjectsOfType<DeadBody>()) if (deadBody.ParentId == id) return deadBody;
        return null;
    }

    internal static bool IsTransformNull(this Transform tr)
    {
        if (tr is null) return true;
        else return false;
    }

    internal static bool HasImpostorVision(GameData.PlayerInfo player)
    {
        return player.Role.IsImpostor
            || (PlayerControl.LocalPlayer.IsRole(RoleType.Jester) && Jester.HasImpostorVision)
            || (PlayerControl.LocalPlayer.IsRole(RoleType.Madmate) && Madmate.HasImpostorVision)
            || (PlayerControl.LocalPlayer.IsRole(RoleType.Jackal) && Jackal.HasImpostorVision)
            || (PlayerControl.LocalPlayer.IsRole(RoleType.Sidekick) && Sidekick.HasImpostorVision);
    }

    internal static string DeleteHTML(this string name)
    {
        var PlayerName = name.Replace("\n", "").Replace("\r", "");
        while (PlayerName.Contains('<') || PlayerName.Contains('>'))
        {
            PlayerName = PlayerName.Remove(PlayerName.IndexOf("<"), PlayerName.IndexOf(">") - PlayerName.IndexOf("<") + 1);
        }
        return PlayerName;
    }

    internal static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        return hex;
    }

    internal static string GetColorHEX(ClientData Client)
    {
        try
        {
            return ColorToHex(Palette.PlayerColors[Client.ColorId]);
        }
        catch
        {
            return "";
        }
    }
}