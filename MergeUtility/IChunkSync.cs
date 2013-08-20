using System;

namespace net.azirale.civcraft.GeoSharer
{
    public interface IChunkSync : IEquatable<IChunkSync>, IComparable<IChunkSync>
    {
        int X { get; }
        int Z { get; }
        long TimeStamp { get; }
    }
}
