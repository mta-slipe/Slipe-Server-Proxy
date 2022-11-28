using SlipeServer.Packets;
using SlipeServer.Packets.Enums;

namespace SlipeServerProxy.Packets;

public class ArbitraryPacket : Packet
{
    private PacketId packetId;
    public override PacketId PacketId => packetId;

    public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

    public override PacketPriority Priority => PacketPriority.High;

    public byte[] Data { get; set; } = Array.Empty<byte>();

    public ArbitraryPacket(PacketId packetId, byte[] data)
    {
        this.packetId = packetId;
        Data = data;
    }

    public ArbitraryPacket()
    {
        packetId = PacketId.PACKET_ID_SERVER_JOIN;
    }

    public void SetPacketId(PacketId packetId) => this.packetId = packetId;

    public override void Read(byte[] bytes)
    {
        Data = bytes;
    }

    public override byte[] Write()
    {
        return Data;
    }

    public ArbitraryPacket Copy()
    {
        return new ArbitraryPacket(this.PacketId, this.Data);
    }
}