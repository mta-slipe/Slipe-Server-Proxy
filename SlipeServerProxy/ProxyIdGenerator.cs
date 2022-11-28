using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements.IdGeneration;

namespace SlipeServerProxy;

public class ProxyIdGenerator : IElementIdGenerator
{
    private uint idCounter;
    private readonly object idLock = new();

    public ProxyIdGenerator()
    {
        this.idCounter = 120000;
    }

    public uint GetId()
    {
        lock (this.idLock)
        {
            this.idCounter = (this.idCounter + 1) % ElementConstants.MaxElementId;
            if (this.idCounter == 0)
                this.idCounter = 120000;
            return this.idCounter;
        }
    }
}