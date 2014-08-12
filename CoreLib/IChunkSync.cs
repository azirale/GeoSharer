using System;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Interface for an object that has a chunk XZ coordinate for equality
    /// and a timestamp for comparison sorting
    /// </summary>
    public interface IChunkSync : IEquatable<IChunkSync>, IComparable<IChunkSync>
    {
        /// <summary>
        /// Should return chunk X coordinate
        /// </summary>
        int X { get; }
        /// <summary>
        /// Should return chunk Z coordinate
        /// </summary>
        int Z { get; }
        /// <summary>
        /// Should return the timestamp of when the chunk was last
        /// updated with live data. Unix DateTime in milliseconds.
        /// </summary>
        long TimeStamp { get; }
    }
}
