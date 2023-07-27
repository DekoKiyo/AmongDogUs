// Source code from TheOtherRolesGMH

namespace AmongDogUs.Modules;

internal class SynchronizeData
{
    private readonly Dictionary<SynchronizeTag, ulong> dic;

    internal SynchronizeData()
    {
        dic = new();
    }

    internal static void Synchronize(SynchronizeTag tag, byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)ECustomRPC.Synchronize, SendOption.Reliable, -1);
        writer.Write(playerId);
        writer.Write((int)tag);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.Synchronize(playerId, (int)tag);
    }

    internal void SynchronizeRPC(SynchronizeTag tag, byte playerId)
    {
        if (!dic.ContainsKey(tag)) dic[tag] = 0;

        dic[tag] |= (ulong)1 << playerId;
    }

    internal bool Align(SynchronizeTag tag, bool withGhost, bool withSurvivor = true)
    {
        bool result = true;

        dic.TryGetValue(tag, out ulong value);

        foreach (PlayerControl pc in CachedPlayer.AllPlayers)
        {
            if (pc.Data.IsDead ? withGhost : withSurvivor) result &= (value & ((ulong)1 << pc.PlayerId)) != 0;
        }

        return result;
    }

    internal void Reset(SynchronizeTag tag)
    {
        dic[tag] = 0;
    }

    internal void Initialize()
    {
        dic.Clear();
    }
}