using System;
using Substrate;
using Substrate.Core;
using System.IO;
using Substrate.Nbt;
using System.Threading;

namespace net.azirale.civcraft.GeoSharer
{
    class GeoWorldBuilder
    {
        private AnvilWorld World;

        public void UpdateWorld(string folderPath, GeoReader geoReader)
        {
            if (!Directory.Exists(folderPath))
            {
                try { Directory.CreateDirectory(folderPath); }
                catch
                {
                    Console.WriteLine("Unable to create directory, aborting world creation");
                    return;
                }
            }
            World = AnvilWorld.Create(folderPath);
            RegionChunkManager chunkManager = World.GetChunkManager();

            Console.WriteLine("Reading geosharer file...");
            int i = 0;
            int added = 0;
            int updated = 0;
            int skipped = 0;
            int failed = 0;
            foreach (GeoChunk addChunk in geoReader)
            {
                //if (i % 50 == 0) ProgressUpdate(geoReader.GetStatusLine() + " (" + i + " chunks complete)");
                new Thread(() => ProgressUpdate(geoReader)).Start();
                if (i % 250 == 0) chunkManager.Save();
                Result thisResult = InsertChunk(addChunk, chunkManager);
                switch (thisResult)
                {
                    case Result.Added: ++added; break;
                    case Result.Updated: ++updated; break;
                    case Result.Skipped: ++skipped; break;
                    case Result.Failed: ++failed; break;
                    default: break;
                }
                ++i;
            }
            Console.WriteLine("\nFinished reading file.\nAdded " + added + " new chunks.\nUpdated " + updated + " recent chunks.\nSkipped " + skipped + " old chunks.\nFailed on " + failed + " chunks.");
            Console.WriteLine("Saving world files.");
            World.Save();
            Console.WriteLine("Done.");
            return;
        }

        private void ProgressUpdate(GeoReader reader)
        {
            string statusText = reader.GetStatusLine();
            ClearConsoleLine();
            Console.Write(statusText);
        }

        private void ClearConsoleLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        public Result InsertChunk(GeoChunk addChunk, IChunkManager chunkManager)
        {
            ChunkRef newChunk;
            Result value;
            if (chunkManager.ChunkExists(addChunk.X, addChunk.Z))
            {
                newChunk = chunkManager.GetChunkRef(addChunk.X, addChunk.Z);
                value = Result.Updated;
            }
            else
            {
                newChunk = chunkManager.CreateChunk(addChunk.X, addChunk.Z);
                value = Result.Added;
            }
            // check the timestamp, if it has one
            AnvilChunk chunk = newChunk.GetChunkRef() as AnvilChunk;
            TagNodeCompound level = chunk.Tree.Root["Level"] as TagNodeCompound;
            if (level.ContainsKey("GeoTimestamp"))
            {
                long currentTimestamp = level["GeoTimestamp"].ToTagLong().Data;
                if (currentTimestamp >= addChunk.TimeStamp) return Result.Skipped;
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

            foreach (GeoBlock block in addChunk.Blocks)
            {
                switch (block.ID)
                {
                    case 154: break; // ignore hoppers
                    case 158: break; // ignore droppers
                    default:
                        try
                        {
                            newBlocks.SetBlock(block.X, block.Y, block.Z, new AlphaBlock(block.ID, block.Meta));
                        }
                        catch (Exception ex)
                        {
                            ClearConsoleLine();
                            Console.WriteLine("Block error: " + ex.Message);
                            newBlocks.SetBlock(block.X, block.Y, block.Z, new AlphaBlock(0));
                        }
                        break;
                }
            }
            // add the biomes
            level["Biomes"] = new TagNodeByteArray(addChunk.Biomes.CopyArray());
            newChunk.SetChunkRef(chunk);
            // flag changes
            newChunk.IsTerrainPopulated = true;
            newChunk.IsDirty = true;
            // recalculate lighting and everything
            newBlocks.RebuildHeightMap();
            newBlocks.RebuildSkyLight();
            newBlocks.RebuildBlockLight();
            newBlocks.StitchSkyLight();
            newBlocks.StitchBlockLight();
            //done
            return value;
        }
    }

    public enum Result { Added, Updated, Skipped, Failed }
}
