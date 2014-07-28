using System;
using System.Collections.Generic;
using System.IO;
using Substrate;
using Substrate.Nbt;

namespace net.azirale.geosharer.core
{
    public class GeoWorldWriter : IMessageSender, IProgressSender
    {
        public int Added { get;  private set; }
        public int Updated { get; private set; }
        public int Skipped { get; private set; }

        public void UpdateWorld(string worldPath, List<GeoChunkRaw> chunks)
        {
            if (!this.CreateOrLoadWorld(worldPath)) return;
            AnvilWorld world = AnvilWorld.Create(worldPath);
            SetWorldDefaults(world);
            // Break chunks into regions for better disk access
            IEnumerable<GeoRegion> regions = this.GetRegionBreakout(chunks);
            RegionChunkManager rcm = world.GetChunkManager();
            long total = chunks.Count;
            long current = 0;
            this.Added = 0;
            this.Updated = 0;
            this.Skipped = 0;
            foreach (GeoRegion region in regions)
            {
                foreach (GeoChunkRaw chunk in region.Chunks)
                {
                    if (rcm.ChunkExists(chunk.X, chunk.Z))
                    {
                        // check the timestamp, if it has one
                        AnvilChunk c = rcm.GetChunk(chunk.X,chunk.Z) as AnvilChunk;
                        TagNodeCompound level = c.Tree.Root["Level"] as TagNodeCompound;
                        if (level.ContainsKey("GeoTimestamp"))
                        {
                            long existingTimestamp = level["GeoTimestamp"].ToTagLong().Data;
                            if (existingTimestamp > chunk.TimeStamp) { this.Skipped++; continue; }
                        }
                        this.Updated++;
                    }
                    else
                    {
                        this.Added++;
                    }
                    AnvilChunk ac = chunk.GetAnvilChunk();
                    rcm.DeleteChunk(chunk.X, chunk.Z);
                    rcm.SetChunk(chunk.X, chunk.Z, ac);
                    ChunkRef cr = rcm.GetChunkRef(chunk.X, chunk.Z);
                    AlphaBlockCollection blocks = cr.Blocks;
                    try
                    {
                        blocks.RebuildHeightMap();
                        blocks.RebuildFluid();
                        blocks.RebuildBlockLight();
                        blocks.RebuildSkyLight();
                    }
                    catch (Exception ex)
                    {
                        this.SendMessage(MessageVerbosity.Error, "Exception rebuilding height/fluid/lighting for Chunk X=" + chunk.X + " Y=" + chunk.Z + " in GeoWorldWriter.UpdateWorld, rethrowing: " + ex.Message);
                    }
                    try
                    {
                        blocks.StitchBlockLight();
                        blocks.StitchSkyLight();
                    }
                    catch (Exception ex)
                    {
                        this.SendMessage(MessageVerbosity.Error, "Exception stitching lighting for Chunk X=" + chunk.X + " Y=" + chunk.Z +  " in GeoWorldWriter.UpdateWorld, rethrowing: " + ex.Message);
                    }
                    current++;
                    this.SendProgress(current, total, "Done chunk X=" + chunk.X + " Z=" + chunk.Z);
                }
                //rcm.RelightDirtyChunks(); // object reference not set?
                try
                {
                    this.SendMessage(MessageVerbosity.Normal, "Saving Region");
                    rcm.Save(); // save to disk, we are done with this region
                }
                catch (Exception ex)
                {
                    this.SendMessage(MessageVerbosity.Error, "Exception saving region in GeoWorldWriter.UpdateWorld, rethrowing: " + ex.Message);
                    throw;
                }
            }
            // all regions done
            try
            {
                this.SendMessage(MessageVerbosity.Normal, "Saving World");
                world.Save();
            }
            catch (Exception ex)
            {
                this.SendMessage(MessageVerbosity.Error, "Exception saving world in GeoWorldWriter.UpdateWorld; rethrowing: " + ex.Message);
                throw;
            }
        }

        private void SetWorldDefaults(AnvilWorld world)
        {
            Level level = world.Level;
            level.AllowCommands = true; // Allow cheats
            level.GameRules.CommandBlockOutput = false; // No command blocks... ?
            level.GameRules.DoFireTick = false; // Not fire spread
            level.GameRules.DoMobSpawning = false; // no mobs
            level.GameRules.MobGriefing = false; // no creeper damage to environment
            level.GameType = GameType.CREATIVE; // creative mode
            level.LastPlayed = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds; // update the last played time, java style
            level.GeneratorName = "flat"; // superflat for generated chunks if loaded in MC
        }

        private IEnumerable<GeoRegion> GetRegionBreakout(List<GeoChunkRaw> chunks)
        {
            Dictionary<Tuple<int,int>,GeoRegion> regions = new Dictionary<Tuple<int,int>,GeoRegion>();
            for (int i = 0; i < chunks.Count; ++i)
            {
                //bool inserted = false;
                GeoChunkRaw chunk = chunks[i];
                Tuple<int, int> regionCode = new Tuple<int, int>(chunk.RX, chunk.RZ);
                GeoRegion region;
                if (!regions.TryGetValue(regionCode, out region)) { region = new GeoRegion(regionCode.Item1, regionCode.Item2); regions[regionCode] = region; }
                region.Chunks.Add(chunk);
                //foreach (GeoRegion region in regions)
                //{
                //    if (chunk.RX == region.X && chunk.RZ == region.Z)
                //    {
                //        region.Chunks.Add(chunk);
                //        inserted = true;
                //        continue;
                //    }
                //}
                //if (!inserted)
                //{
                //    GeoRegion region = new GeoRegion(chunk.RX, chunk.RZ);
                //    region.Chunks.Add(chunk);
                //    regions.Add(region);
                //}
            }
            return regions.Values;
        }

        private bool CreateOrLoadWorld(string worldPath)
        {
            // Try to find the folder first, if it doesn't exist try and create it, if that fails then end with an error message
            if (!Directory.Exists(worldPath))
            {
                this.SendMessage(MessageVerbosity.Normal, "WorldBuilder.UpdateWorld(): Could not find directory, attempting to create");
                try
                {
                    Directory.CreateDirectory(worldPath);
                    this.SendMessage(MessageVerbosity.Normal, "WorldBuilder.UpdateWorld(): Created new world directory");
                }
                catch
                {
                    this.SendMessage(MessageVerbosity.Error, "WorldBuilder.UpdateWorld(): Unable to create directory, aborting method");
                    return false;
                }
            }
            return true;
        }




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
