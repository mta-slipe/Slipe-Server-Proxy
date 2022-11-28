using SlipeServer.Packets.Enums;
using SlipeServer.Server;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServerProxy.Packets.RelayServices;

namespace SlipeServerProxy.Packets;

public class ArbitraryPacketHandler : IPacketHandler<ArbitraryPacket>
{
    private readonly PacketId packetId;
    public PacketId PacketId => packetId;

    private readonly IncomingPacketRelayService relayService;

    public ArbitraryPacketHandler(PacketId packetId, IncomingPacketRelayService relayService)
    {
        this.packetId = packetId;
        this.relayService = relayService;
    }

    public void HandlePacket(IClient client, ArbitraryPacket packet)
    {
        packet.SetPacketId(packetId);
        relayService.RelayPacket(client, packet);
    }
}