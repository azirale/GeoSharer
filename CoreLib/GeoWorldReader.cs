using Substrate;
using Substrate.Nbt;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Reads world save data, does not make any modifications to existing world saves
    /// </summary>
    public class GeoWorldReader : IMessageSender, IProgressSender
    {
        private readonly string worldPath;

        /// <summary>
        /// Create a new GeoWorldReader with a target directory path for the world save
        /// </summary>
        /// <param name="worldPath">Directory path to the world save</param>
        public GeoWorldReader(string worldPath)
        {

            this.worldPath = worldPath;
        }

        #region Get World Stats
        /// <summary>
        /// Subclass tuple-like to return both the number of regions and number of chunks in a world save
        /// </summary>
        public class StatsResults
        {
            /// <summary>
            /// Number of regions in the world save
            /// </summary>
            public readonly int NumberOfRegions;
            /// <summary>
            /// Number of chunks in the world save
            /// </summary>
            public readonly int NumberOfChunks;
            /// <summary>
            /// Create a new StatsResults with a given number for regions and chunks in a world save
            /// </summary>
            /// <param name="numberOfRegions">Number of regions in the world save</param>
            /// <param name="numberOfChunks">Number of chunks in the world save</param>
            public StatsResults(int numberOfRegions, int numberOfChunks)
            {
                this.NumberOfRegions = numberOfRegions;
                this.NumberOfChunks = numberOfChunks;
            }
        }

        /// <summary>
        /// Read through all of the region files in a world save to count how many regions and chunks it contains
        /// </summary>
        /// <returns></returns>
        public StatsResults GetWorldStats()
        {
            AnvilWorld world = AnvilWorld.Open(worldPath);
            if (world == null) return new StatsResults(0, 0);
            AnvilRegionManager rm = world.GetRegionManager();
            int numberOfRegions = 0;
            int numberOfChunks = 0;
            foreach (AnvilRegion r in world.GetRegionManager())
            {
                numberOfRegions++;
                numberOfChunks += r.ChunkCount();
            }
            return new StatsResults(numberOfRegions, numberOfChunks);
        }
        #endregion

        #region Extract raw geosharer chunk data
        /// <summary>
        /// Read through the region files in this world save and pull raw geosharer chunks,
        /// returning all of the data in a single list all together.
        /// </summary>
        /// <returns>A list of all geosharer raw chunk data in the world save</returns>
        public List<GeoChunkRaw> ExtractAllRawChunks()
        {
            List<GeoChunkRaw> chunks = new List<GeoChunkRaw>();
            AnvilWorld world = AnvilWorld.Open(this.worldPath);
            AnvilRegionManager arm = world.GetRegionManager();
            foreach (AnvilRegion ar in arm)
            {
                for (int x = 0; x < 32; x++)
                {
                    for (int z = 0; z < 32; z++)
                    {
                        if (!ar.ChunkExists(x, z)) continue;
                        NbtTree tree = ar.GetChunkTree(z, x);
                        GeoChunkRaw raw = new GeoChunkRaw(tree);
                        chunks.Add(raw);
                    }
                }
            }
            return chunks;
        }

        /// <summary>
        /// Read through the region files in this world save and pull raw geosharer chunks
        /// one by one. Has reduced memory requirements compared to returning all the chunk data
        /// at once
        /// </summary>
        /// <returns>A yield enumerator of GeoChunkRaw objects</returns>
        public IEnumerable<GeoChunkRaw> EnumerateRawChunks()
        {
            AnvilWorld world = AnvilWorld.Open(this.worldPath);
            AnvilRegionManager arm = world.GetRegionManager();
            foreach (AnvilRegion ar in arm)
            {
                for (int x = 0; x < 32; x++)
                {
                    for (int z = 0; z < 32; z++)
                    {
                        if (!ar.ChunkExists(x, z)) continue;
                        NbtTree tree = ar.GetChunkTree(z, x);
                        GeoChunkRaw raw = new GeoChunkRaw(tree);
                        yield return raw;
                    }
                }
            }
        }
        #endregion

        #region IMessageSender Implementation
        /// <summary>
        /// Subscribe to this event to receive text messages from the world builder object. 
        /// </summary>
        public event Message Messaging;

        /// <summary>
        /// Internal method of the WorldBuilder to proc a Messaging event
        /// </summary>
        /// <param name="channel">Which verbosity channel the message should be sent on</param>
        /// <param name="text">The text of the message</param>
        private void SendMessage(MessageChannel channel, string text)
        {
            Message msg = this.Messaging;
            if (msg != null) msg(this, new MessagePacket(channel, text));
        }

        /// <summary>
        /// Returns the list of subscribers to the messaging event of this WorldBuilder object
        /// </summary>
        /// <returns>Subscribers to Messaging event</returns>
        public Message GetMessagingList()
        {
            return this.Messaging;
        }
        #endregion

        #region IProgressSender Implementation
        /// <summary>
        /// Subscribe to this event to receive progress updates from the world builder object
        /// </summary>
        public event Progress Progressing;

        /// <summary>
        /// Internal method of the WorldBuilder to proc a Progressing event
        /// </summary>
        /// <param name="current">Current progress</param>
        /// <param name="maximum">Maximum progress</param>
        /// <param name="text">Accompanying text for this progress</param>
        private void SendProgress(long current, long maximum, string text)
        {
            Progress prg = this.Progressing;
            if (prg != null) prg(this, new ProgressPacket(current, maximum, text));
        }
        #endregion
    }
}