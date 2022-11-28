using SlipeServer.Server;
using SlipeServerProxy.Packets;

namespace SlipeServerProxy.Remotes;

public class PacketRemoteMessage : IRemoteMessage
{
    private readonly byte[] data;

    public PacketRemoteMessage(IClient client, ArbitraryPacket packet)
    {
        var id = (client.Player as ProxyPlayer)!.NetId;
        var idBytes = BitConverter.GetBytes(id);

        this.data = new byte[6 + packet.Data.Length + idBytes.Length];

        uint length = Convert.ToUInt32(packet.Data.Length + idBytes.Length + 1);
        var lengthBytes = BitConverter.GetBytes(length);

        data[0] = (byte)RemoteMessageType.packet;
        lengthBytes.CopyTo(data, 1);
        idBytes.CopyTo(data, 5);
        data[9] = (byte)packet.PacketId;
        packet.Data.CopyTo(data, 6 + idBytes.Length);
    }

    public byte[] GetBytes()
    {
        return this.data;
    }
}