using Substrate;

namespace net.azirale.geosharer.core
{
    public class GeoWorldReader : IMessageSender, IProgressSender
    {
        private readonly string worldPath;

        public GeoWorldReader(string worldPath)
        {
            this.worldPath = worldPath;
        }

        #region Get World Stats
        public class StatsResults
        {
            public readonly int NumberOfRegions;
            public readonly int NumberOfChunks;
            public StatsResults(int numberOfRegions, int numberOfChunks)
            {
                this.NumberOfRegions = numberOfRegions;
                this.NumberOfChunks = numberOfChunks;
            }
        }

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


        #region IMessageSender Implementation
        /// <summary>
        /// Subscribe to this event to receive text messages from the world builder object. 
        /// </summary>
        public event Message Messaging;

        /// <summary>
        /// Internal method of the WorldBuilder to proc a Messaging event
        /// </summary>
        /// <param name="verbosity">Which verbosity channel the message should be sent on</param>
        /// <param name="text">The text of the message</param>
        private void SendMessage(MessageVerbosity verbosity, string text)
        {
            Message msg = this.Messaging;
            if (msg != null) msg(this, new MessagePacket(verbosity, text));
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