using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Server;
using SlipeServer.Server.Elements.IdGeneration;
using SlipeServer.Server.Loggers;
using SlipeServerProxy;
using SlipeServerProxy.Loggers;
using SlipeServerProxy.Packets.RelayServices;
using SlipeServerProxy.Remotes;
using SlipeServerProxy.Services;

var configuration = new Configuration()
{
    Port = 51666,
    DebugPort = 51667,
};

var server = MtaServer.Create<ProxyPlayer>(builder =>
{
    builder.UseConfiguration(configuration);

    builder.ConfigureServices(services =>
    {
        services.AddSingleton<ILogger, ProxyLogger>();
        services.AddSingleton<RemoteServer>();
        services.AddSingleton<IncomingPacketRelayService>();
        services.AddSingleton<OutgoingPacketRelayService>();
        services.AddSingleton<KnownResourcesService>();
        services.AddSingleton<ResetService>();
        services.AddSingleton<IElementIdGenerator, ProxyIdGenerator>();
    });

    foreach (var packet in Enum.GetValues<PacketId>())
        builder.AddArbitraryPacketHandler(packet);

    builder.AddConnectionPacketHandlers();
    builder.AddNetWrappers();

    builder.AddLogic<JoinQuitLogic>();
});

server.Start();

server.GetRequiredService<ILogger>().LogInformation("Proxy started");
await Task.Delay(-1);