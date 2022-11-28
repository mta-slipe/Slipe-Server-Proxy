using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.PacketHandling.Handlers.Connection;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.ServerBuilders;
using SlipeServerProxy.Packets;

namespace SlipeServerProxy;

public static class ServerBuilderExtensions
{
    public static void AddArbitraryPacketHandler(this ServerBuilder builder, PacketId packetId)
    {
        builder.AddBuildStep(server =>
        {
            var handler = server.Instantiate<ArbitraryPacketHandler>(packetId);
            var queueHandler = server.Instantiate<ScalingPacketQueueHandler<ArbitraryPacket>>(handler);

            server.RegisterPacketHandler(packetId, queueHandler);
        });
    }

    public static void AddConnectionPacketHandlers(this ServerBuilder builder)
    {
        builder.AddPacketHandler<JoinedGamePacketHandler, JoinedGamePacket>();
        builder.AddPacketHandler<JoinDataPacketHandler, PlayerJoinDataPacket>();
        builder.AddPacketHandler<PlayerQuitPacketHandler, PlayerQuitPacket>();
        builder.AddPacketHandler<PlayerTimeoutPacketHandler, PlayerTimeoutPacket>();
        builder.AddPacketHandler<RpcPacketHandler, RpcPacket>();
    }

    public static void AddNetWrappers(this ServerBuilder builder)
    {
        builder.AddNetWrapper(
            Directory.GetCurrentDirectory(),
            "net",
            builder.Configuration.Host,
            builder.Configuration.Port,
            builder.Configuration.AntiCheat);

        if (builder.Configuration.DebugPort.HasValue)
            builder.AddNetWrapper(dllPath: "net_d", port: builder.Configuration.DebugPort.Value);
    }
}