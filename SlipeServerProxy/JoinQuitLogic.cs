using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;

namespace SlipeServerProxy;

public class JoinQuitLogic
{
    private readonly ILogger logger;

    public JoinQuitLogic(MtaServer server, ILogger logger)
    {
        this.logger = logger;

        server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        this.logger.LogInformation("{name} joined", player.Name);

        player.Disconnected += HandlePlayerDisconnect;
    }

    private void HandlePlayerDisconnect(Player sender, SlipeServer.Server.Elements.Events.PlayerQuitEventArgs e)
    {

    }
}