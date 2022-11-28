using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using SlipeServer.Server;
using SlipeServer.Server.Extensions;
using SlipeServerProxy.Services;

namespace SlipeServerProxy.Packets.RelayServices;

public class OutgoingPacketRelayService
{
    private readonly MtaServer<ProxyPlayer> server;
    private readonly KnownResourcesService knownResourcesService;
    private readonly ILogger logger;

    private Dictionary<uint, ProxyPlayer> players;

    private readonly HashSet<PacketId> bannedPackets = new()
    {
        PacketId.PACKET_ID_SERVER_JOIN_COMPLETE,
        PacketId.PACKET_ID_SERVER_JOINEDGAME,
        PacketId.PACKET_ID_MOD_NAME,
    };

    public OutgoingPacketRelayService(ILogger logger, MtaServer<ProxyPlayer> server, KnownResourcesService knownResourcesService)
    {
        this.server = server;
        this.knownResourcesService = knownResourcesService;
        this.logger = logger;

        players = new();

        server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(ProxyPlayer player)
    {
        players[player.NetId] = player;

        player.Disconnected += (e, a) => players.Remove(player.NetId);
    }

    public void RelayPacket(uint playerId, ArbitraryPacket packet)
    {
        if (packet.PacketId == PacketId.PACKET_ID_RESOURCE_START)
            HandleJoinedgamePacket(playerId, packet);

        if (bannedPackets.Contains(packet.PacketId))
            return;

        if (players.TryGetValue(playerId, out var player))
            packet.SendTo(player);
    }

    private void HandleJoinedgamePacket(uint playerId, ArbitraryPacket packet)
    {
        var reader = new PacketReader(packet.Data);
        var nameLength = reader.GetByte();
        var name = reader.GetBytes(nameLength);
        var netId = reader.GetUint16();
        this.knownResourcesService.RegisterResource(netId);
    }
}