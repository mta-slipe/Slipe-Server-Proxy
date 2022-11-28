using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServerProxy.Packets;
using SlipeServerProxy.Packets.RelayServices;
using SlipeServerProxy.Services;
using System.IO.Pipes;

namespace SlipeServerProxy.Remotes;

public class RemoteServer
{
    private readonly OutgoingPacketRelayService outgoingRelayer;
    private readonly ResetService resetService;
    private readonly IElementCollection elementCollection;
    private readonly ILogger logger;
    private NamedPipeServerStream namedPipe;

    public RemoteServer(
        OutgoingPacketRelayService outgoingRelayer,
        ResetService resetService,
        IElementCollection elementCollection,
        ILogger logger)
    {
        this.outgoingRelayer = outgoingRelayer;
        this.resetService = resetService;
        this.elementCollection = elementCollection;
        this.logger = logger;

        this.namedPipe = new("mta-server-proxy", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        Init();
    }

    public async void Init()
    {
        while (true)
        {
            try
            {
                if (!this.namedPipe.IsConnected)
                {
                    this.resetService.Reset();
                    await this.namedPipe.WaitForConnectionAsync();
                    InitListen();
                }
            }
            catch (IOException e)
            {
                this.logger.LogError("Error connecting {error}", e.Message);
                this.namedPipe.Close();
                this.namedPipe.Dispose();
                this.namedPipe = new("mta-server-proxy", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            }

            await Task.Delay(500);
        }
    }

    public void SendMessage(IRemoteMessage message)
    {
        var buffer = message.GetBytes();
        try
        {
            if (this.namedPipe.IsConnected)
                this.namedPipe.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (IOException e)
        {
            this.logger.LogError("Error sending {error}", e.Message);
        }
    }

    private async void InitListen()
    {
        this.logger.LogInformation("Connection successful, listening");
        RelayCapturedPackets();

        byte[] previous = Array.Empty<byte>();
        byte[] buffer = new byte[10240];
        try
        {
            while (true)
            {
                var size = await this.namedPipe.ReadAsync(buffer);

                if (size > 0)
                {
                    previous = HandleData(previous.Concat(buffer.Take(size)).ToArray());
                }
            }

        }
        catch (Exception e)
        {
            this.logger.LogError("{error}", e.Message);
        }
    }

    private byte[] HandleData(byte[] data)
    {
        if (data.Length < 5)
            return data;

        var type = (RemoteMessageType)data[0];
        var size = BitConverter.ToUInt32(data, 1);

        if (data.Length < size + 5)
            return data;

        var payload = data.Skip(5).Take((int)size);
        HandleMessage(type, payload.ToArray());

        return HandleData(data.Skip((int)size + 5).ToArray());
    }

    private void HandleMessage(RemoteMessageType type, byte[] data)
    {
        if (type == RemoteMessageType.packet)
        {
            var playerId = BitConverter.ToUInt32(data, 0);
            var packetId = (PacketId)data[4];
            this.logger.LogTrace("<-- {type} from {id}", packetId, playerId);

            var payload = data.Skip(5);
            var packet = new ArbitraryPacket(packetId, payload.ToArray());

            this.outgoingRelayer.RelayPacket(playerId, packet);
        }
    }

    private void RelayCapturedPackets()
    {
        foreach (var player in this.elementCollection.GetByType<ProxyPlayer>())
            Task.Run(async () =>
            {
                foreach (var packet in player.CapturedPackets)
                {
                    logger.LogTrace("--> {packet} from {client}", packet.PacketId, player.Name);
                    this.SendMessage(new PacketRemoteMessage(player.Client, packet));
                    await Task.Delay(250);
                }
            });
    }
}