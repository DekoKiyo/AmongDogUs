namespace AmongDogUs.Modules;

[HarmonyPatch]
internal static class DynamicLobbies
{
    internal static int LobbyLimit = 15;
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    private static class SendChatPatch
    {
        static bool Prefix(ChatController __instance)
        {
            string text = __instance.freeChatField.Text;
            bool handled = false;
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            {
                if (text.ToLower().StartsWith("/size "))
                {
                    // Unfortunately server holds this - need to do more trickery
                    if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan())
                    {
                        // checking both just cause
                        handled = true;
                        if (!int.TryParse(text.AsSpan(6), out LobbyLimit))
                        {
                            __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, ModResources.CommandErrorSize);
                        }
                        else
                        {
                            LobbyLimit = Math.Clamp(LobbyLimit, 4, 15);
                            if (LobbyLimit != GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers)
                            {
                                GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers = LobbyLimit;
                                FastDestroyableSingleton<GameStartManager>.Instance.LastPlayerCount = LobbyLimit;
                                CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.currentGameOptions));
                                __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, string.Format(ModResources.LobbySizeChanged, LobbyLimit));
                            }
                            else __instance.AddChat(CachedPlayer.LocalPlayer.PlayerControl, string.Format(ModResources.LobbySizeAlready, LobbyLimit));
                        }
                    }
                }
            }
            if (handled)
            {
                __instance.freeChatField.Clear();
                __instance.quickChatMenu.Clear();
            }
            return !handled;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
    internal static class InnerNetClientHostPatch
    {
        internal static void Prefix(InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings)
        {
            int maxPlayers;
            try
            {
                maxPlayers = GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers;
            }
            catch
            {
                maxPlayers = 15;
            }
            LobbyLimit = maxPlayers;
            settings.MaxPlayers = 15; // Force 15 Player Lobby on Server
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }

        internal static void Postfix(InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings)
        {
            settings.MaxPlayers = LobbyLimit;
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    internal static class InnerNetClientJoinPatch
    {
        internal static void Prefix(InnerNetClient __instance)
        {
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    internal static class AmongUsClientOnPlayerJoined
    {
        internal static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (LobbyLimit < __instance.allClients.Count)
            {
                DisconnectPlayer(__instance, client.Id);
                return false;
            }
            return true;
        }

        private static void DisconnectPlayer(InnerNetClient _this, int clientId)
        {
            if (!_this.AmHost)
            {
                return;
            }
            MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
            messageWriter.StartMessage(4);
            messageWriter.Write(_this.GameId);
            messageWriter.WritePacked(clientId);
            messageWriter.Write((byte)DisconnectReasons.GameFull);
            messageWriter.EndMessage();
            _this.SendOrDisconnect(messageWriter);
            messageWriter.Recycle();
        }
    }
}