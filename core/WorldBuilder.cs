using System;
using Substrate;
using Substrate.Core;
using System.IO;
using Substrate.Nbt;
using System.Threading;

namespace net.azirale.geosharer.core
{
    public class WorldBuilder : IMessageSender, IProgressSender
    {
        /***** CONSTRUCTOR MEMBERS **************************************************************/
        #region Constructor Members
        /// <summary>
        /// Create a new empty WorldBuilder object
        /// </summary>
        public WorldBuilder() { }
        #endregion

        /***** PRIVATE FIELDS *******************************************************************/
        #region Private Fields
        // temporary to find out what takes so long when bringing stuff in
        private long timer1; // read chunk
        private long timer2; // set blocks
        private long timer3; // stitch chunks
        #endregion

        /***** PUBLIC METHODS *******************************************************************/
        #region Public Methods

        /// <summary>
        /// Updates or creates a minecraft world with data from a GeoReader object
        /// </summary>
        /// <param name="folderPath">Directory path to the world - the directory the level.dat file would be in</param>
        /// <param name="geoReader">A GeoReader object that has .geosharer files attached</param>
        public void UpdateWorld(string folderPath, GeoReader geoReader)
        {
            System.Diagnostics.Stopwatch bigclock = new System.Diagnostics.Stopwatch();
            bigclock.Start();
            // Try to find the folder first, if it doesn't exist try and create it, if that fails then end with an error message
            if (!Directory.Exists(folderPath))
            {
                this.SendMessage(MessageVerbosity.Normal, "WorldBuilder.UpdateWorld(): Could not find directory, attempting to create");
                try
                {
                    Directory.CreateDirectory(folderPath);
                    this.SendMessage(MessageVerbosity.Normal, "WorldBuilder.UpdateWorld(): Created new world directory");
                }
                catch
                {
                    this.SendMessage(MessageVerbosity.Error, "WorldBuilder.UpdateWorld(): Unable to create directory, aborting method");
                    return;
                }
            }
            // Prepare to update the minecraft world
            AnvilWorld world = AnvilWorld.Create(folderPath);
            // Configure a few default settings for the world
            Level level = world.Level;
            level.AllowCommands = true; // Allow cheats
            level.GameRules.CommandBlockOutput = false; // No command blocks... ?
            level.GameRules.DoFireTick = false; // Not fire spread
            level.GameRules.DoMobSpawning = false; // no mobs
            level.GameRules.MobGriefing = false; // no creeper damage to environment
            level.GameType = GameType.CREATIVE; // creative mode
            level.LastPlayed = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds; // update the last played time, java style
            level.GeneratorName = "Superflat"; // superflat for generated chunks if loaded in MC
            RegionChunkManager chunkManager = world.GetChunkManager();
            int i = 0; // counter for the number of chunks handled
            int added = 0; // counter how many new chunks were added to the world
            int updated = 0; // counter for how many existing chunks were updated
            int skipped = 0; // counter for how many old chunks were skipped
            int failed = 0; // counter for how many chunk updates were aborted due to an error
            this.SendMessage(MessageVerbosity.Normal, "Reading GeoSharer files...");
            ProgressWatcher watcher = new ProgressWatcher(geoReader, this);
            watcher.Start();
            // profiling
            System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
            clock.Start();
            foreach (GeoChunk addChunk in geoReader)
            {
                clock.Stop();
                if (i % 250 == 0) chunkManager.Save();
                ChunkInsertResult thisResult = InsertChunk(addChunk, chunkManager);
                switch (thisResult)
                {
                    case ChunkInsertResult.Added: ++added; break;
                    case ChunkInsertResult.Updated: ++updated; break;
                    case ChunkInsertResult.Skipped: ++skipped; break;
                    case ChunkInsertResult.Failed: ++failed; break;
                    default: break;
                }
                ++i;
                clock.Start();
            }
            clock.Stop();
            this.timer1 = clock.ElapsedMilliseconds;
            watcher.Stop();
            this.SendProgress(geoReader.TotalLength, geoReader.TotalLength, geoReader.GetStatusText());
            this.SendMessage(MessageVerbosity.Normal, "Finished reading file.\nAdded " + added + " new chunks.\nUpdated " + updated + " recent chunks.\nSkipped " + skipped + " old chunks.\nFailed on " + failed + " chunks.");
            this.SendMessage(MessageVerbosity.Normal, "Saving world files.");
            world.Save();
            this.SendMessage(MessageVerbosity.Normal, "Done.");
            bigclock.Stop();
            this.SendMessage(MessageVerbosity.Debug, "Read chunk ms: " + this.timer1.ToString());
            this.SendMessage(MessageVerbosity.Debug, "Write blocks ms: " + this.timer2.ToString());
            this.SendMessage(MessageVerbosity.Debug, "Stich chunks ms: " + this.timer3.ToString());
            this.SendMessage(MessageVerbosity.Debug, "Total time taken: " + bigclock.ElapsedMilliseconds.ToString());
            return;
        }

        #endregion

        /***** PRIVATE METHODS ******************************************************************/
        #region Private Methods
        /// <summary>
        /// Attempts to insert a given GeoChunk into the world using the given IChunkManager while
        /// giving preference to whichever is the most up to date
        /// </summary>
        /// <param name="addChunk">GeoChunk object to insert</param>
        /// <param name="chunkManager">IChunkManager object from substrate</param>
        /// <returns></returns>
        public ChunkInsertResult InsertChunk(GeoChunk addChunk, IChunkManager chunkManager)
        {
            ChunkRef newChunk;
            ChunkInsertResult value;
            if (chunkManager.ChunkExists(addChunk.X, addChunk.Z))
            {
                newChunk = chunkManager.GetChunkRef(addChunk.X, addChunk.Z);
                value = ChunkInsertResult.Updated;
            }
            else
            {
                newChunk = chunkManager.CreateChunk(addChunk.X, addChunk.Z);
                value = ChunkInsertResult.Added;
            }
            // check the timestamp, if it has one
            AnvilChunk chunk = newChunk.GetChunkRef() as AnvilChunk;
            TagNodeCompound level = chunk.Tree.Root["Level"] as TagNodeCompound;
            if (level.ContainsKey("GeoTimestamp"))
            {
                long currentTimestamp = level["GeoTimestamp"].ToTagLong().Data;
                if (currentTimestamp >= addChunk.TimeStamp) return ChunkInsertResult.Skipped;
                level["GeoTimestamp"] = new TagNodeLong(addChunk.TimeStamp);
            }
            else
            {
                level.Add("GeoTimestamp", new TagNodeLong(addChunk.TimeStamp));
            }
            // add the blocks
            AlphaBlockCollection newBlocks = newChunk.Blocks;
            newBlocks.AutoFluid = false;
            newBlocks.AutoLight = false;
            newBlocks.AutoTileTick = false;
            System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
            clock.Start();

            foreach (GeoBlock block in addChunk.Blocks)
            {
                switch (block.ID)
                {
                    case 130: break; // ignore ender chests due to errors from substrate
                    case 154: break; // ignore hoppers due to errors from substrate
                    case 158: break; // ignore droppers due to errors from substrate
                    default:
                        try
                        {
                            newBlocks.SetBlock(block.X, block.Y, block.Z, new AlphaBlock(block.ID, block.Meta));
                        }
                        catch (Exception ex)
                        {
                            this.SendMessage(MessageVerbosity.Error, "Block writing error from substrate: " + ex.Message);
                            newBlocks.SetBlock(block.X, block.Y, block.Z, new AlphaBlock(0));
                        }
                        break;
                }
            }
            
            clock.Stop();
            this.timer2 += clock.ElapsedMilliseconds;
            // add the biomes
            level["Biomes"] = new TagNodeByteArray(addChunk.Biomes.CopyArray());
            newChunk.SetChunkRef(chunk);
            // flag changes
            newChunk.IsTerrainPopulated = true;
            newChunk.IsDirty = true;
            // recalculate lighting and everything
            clock.Restart();

            newBlocks.RebuildHeightMap();
            newBlocks.RebuildSkyLight();
            newBlocks.RebuildBlockLight();
            newBlocks.StitchSkyLight();
            newBlocks.StitchBlockLight();
            /**/
            clock.Stop();
            this.timer3 += clock.ElapsedMilliseconds;
            //done
            return value;
        }

        #endregion

        /***** NESTED CLASS - PROGRESSWATCHER ***************************************************/
        #region Nested Class - ProgressWatcher
        /// <summary>
        /// Internal to the WorldBuilder class, this will keep tabs on the progress and proc progress events periodically
        /// </summary>
        private class ProgressWatcher
        {
            /***** CONSTRUCTOR MEMBERS **************************************************************/
            #region Constructor Members
            /// <summary>
            /// Create a ProgressWatcher that keeps tabs on where the given GeoReader is up to
            /// </summary>
            /// <param name="reader">GeoReader object that the WorldBuilder is using</param>
            /// <param name="parent">The parent WorldBuilder object to this ProgressWatcher</param>
            public ProgressWatcher(GeoReader reader, WorldBuilder parent)
            {
                if (parent == null) throw new ArgumentNullException("parent");
                if (reader == null) throw new ArgumentNullException("reader");
                this.reader = reader;
                this.builder = parent;
                this.inProgress = false;
            }
            #endregion

            /***** PRIVATE FIELDS *******************************************************************/
            #region Private Fields
            /// <summary>
            /// GeoReader object that the WorldBuilder is accessing data from
            /// </summary>
            private GeoReader reader;
            /// <summary>
            /// The parent WorldBuilder object to this ProgressWatcher
            /// </summary>
            private WorldBuilder builder;
            /// <summary>
            /// Tracks whether progress is ongoing
            /// </summary>
            private bool inProgress;
            /// <summary>
            /// How many milliseconds between ticks for progress updates
            /// </summary>
            private const int msPerTick = 500;
            #endregion

            /***** PUBLIC METHODS *******************************************************************/
            #region Public Methods
            /// <summary>
            /// Start this ProgressWatcher object, it will automatically spawn a new thread to make sure progress updates are consistently timed
            /// </summary>
            public void Start()
            {
                lock (this)
                {
                    if (this.inProgress) return; // Abort if this is already in progress
                    this.inProgress = true;
                    new Thread(() => this.Tick()).Start();    
                }
            }

            /// <summary>
            /// Stop this ProgressWatcher object , it will stop checking for progress updates from the GeoReader
            /// </summary>
            public void Stop()
            {
                lock (this)
                {
                    this.inProgress = false;
                }
            }
            #endregion

            /***** PRIVATE METHODS ******************************************************************/
            #region Private Methods
            /// <summary>
            /// The periodic tick of this object, checks in with the GeoReader for the current progress
            /// </summary>
            private void Tick()
            {
                while (true)
                {
                    lock (this)
                    {
                        if (!this.inProgress) break;
                    }
                    this.ProgressUpdate();
                    Thread.Sleep(msPerTick);
                }
            }

            /// <summary>
            /// Asks the GeoReader object for its status
            /// </summary>
            private void ProgressUpdate()
            {
                string statusText = this.reader.GetStatusText();
                long current = this.reader.CurrentPosition;
                long total = this.reader.TotalLength;
                builder.SendProgress(current, total, statusText);
            }
            #endregion
        }
        #endregion

        /***** IMESSAGESENDER IMPLEMENTATION ****************************************************/
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

        /***** IPROGRESSSENDER IMPLEMENTATION ***************************************************/
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
