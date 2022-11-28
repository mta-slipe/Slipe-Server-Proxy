using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Extensions;

namespace SlipeServerProxy.Services;

public class KnownResourcesService
{
    private readonly HashSet<ushort> resourceIds;
    private readonly IElementCollection elementCollection;

    public IEnumerable<ushort> ResourceIds => this.resourceIds.ToArray();

    public KnownResourcesService(IElementCollection elementCollection)
    {
        this.resourceIds = new();
        this.elementCollection = elementCollection;
    }

    public void RegisterResource(ushort id)
    {
        this.resourceIds.Add(id);
    }

    public void ClearResources()
    {
        var players = this.elementCollection.GetByType<ProxyPlayer>();
        foreach (var resource in this.resourceIds)
        {
            new ResourceStopPacket(resource)
                .SendTo(players);
        }
    }
}
