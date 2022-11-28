using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Lua.Rpc.Destroys;
using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;

namespace SlipeServerProxy.Services;

public class ResetService
{
    private readonly HashSet<ushort> resourceIds;
    private readonly IElementCollection elementCollection;
    private readonly KnownResourcesService knownResourcesService;

    public IEnumerable<ushort> ResourceIds => this.resourceIds.ToArray();

    public ResetService(
        IElementCollection elementCollection,
        KnownResourcesService knownResourcesService
    )
    {
        this.resourceIds = new();
        this.elementCollection = elementCollection;
        this.knownResourcesService = knownResourcesService;
    }

    public void Reset()
    {
        this.knownResourcesService.ClearResources();

        var players = this.elementCollection.GetByType<ProxyPlayer>();
        foreach (var player in players)
            ResetPlayer(player, players);

        new DestroyAllBlipsRpcPacket().SendTo(players);
        new DestroyAllMarkersRpcPacket().SendTo(players);
        new DestroyAllPickupsRpcPacket().SendTo(players);
        new DestroyAllRadarAreasRpcPacket().SendTo(players);
        new DestroyAllVehiclesRpcPacket().SendTo(players);
        new DestroyAllWorldObjectsRpcPacket().SendTo(players);
    }

    private void ResetPlayer(ProxyPlayer player, IEnumerable<ProxyPlayer> allPlayers)
    {
        var others = allPlayers.Where(x => x != player);
        foreach (var otherPlayer in others)
        {
            new PlayerQuitPacket(otherPlayer.Id, 0).SendTo(player);
        }
    }
}
