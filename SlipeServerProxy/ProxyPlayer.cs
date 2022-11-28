using SlipeServer.Server.Elements;
using SlipeServerProxy.Packets;

namespace SlipeServerProxy;

public class ProxyPlayer : Player
{
    private readonly List<ArbitraryPacket> capturedPackets;
    public IEnumerable<ArbitraryPacket> CapturedPackets => this.capturedPackets.AsReadOnly();

    public uint NetId => this.Id;

    public ProxyPlayer()
    {
        this.capturedPackets = new();
    }

    public void CapturePacket(ArbitraryPacket packet) => this.capturedPackets.Add(packet.Copy());
}