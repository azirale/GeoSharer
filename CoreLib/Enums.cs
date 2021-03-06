﻿
namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Result of trying to insert a chunk into a world
    /// </summary>
    public enum ChunkInsertResult { Added, Updated, Skipped, Failed }

    /// <summary>
    /// Channel for messages - see IMessageSender interface
    /// </summary>
    public enum MessageChannel { Error, Quiet, Normal, Verbose, Debug }
}
