using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Server;
using SlipeServerProxy.Remotes;

namespace SlipeServerProxy.Packets.RelayServices;

public class IncomingPacketRelayService
{
    private readonly ILogger logger;
    private readonly RemoteServer remoteServer;

    private readonly HashSet<PacketId> packetsToCapture = new HashSet<PacketId>()
    {
        PacketId.PACKET_ID_PLAYER_JOIN,
        PacketId.PACKET_ID_PLAYER_JOINDATA,
        PacketId.PACKET_ID_RPC
    };

    public IncomingPacketRelayService(ILogger logger, RemoteServer remoteServer)
    {
        this.logger = logger;
        this.remoteServer = remoteServer;
    }

    public void RelayPacket(IClient client, ArbitraryPacket packet)
    {
        logger.LogTrace("--> {packet} from {client}", packet.PacketId, client.Player.Name);

        remoteServer.SendMessage(new PacketRemoteMessage(client, packet));

        if (this.packetsToCapture.Contains(packet.PacketId))
            (client.Player as ProxyPlayer)!.CapturePacket(packet);
    }
}