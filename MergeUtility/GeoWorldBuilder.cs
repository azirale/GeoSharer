using System;
using System.Collections.Generic;
using Substrate;
using Substrate.Core;
using System.IO;
using Substrate.Nbt;

namespace net.azirale.civcraft.GeoSharer
{
    class GeoWorldBuilder
    {
        private AnvilWorld World;

        public bool CreateWorld(string folderPath, List<GeoChunk> addChunks)
        {
            if (!Directory.Exists(folderPath)) try { Directory.CreateDirectory(folderPath); }
                catch
                {
                    Console.WriteLine("Unable to create directory, aborting world creation");
                    return false;
                }
            World = AnvilWorld.Create(folderPath);
            Console.WriteLine("Sorting chunks by position. Using " + addChunks.Count + " chunks");
            addChunks.Sort();
            RegionChunkManager chunkManager = World.GetChunkManager();
            foreach (GeoChunk addChunk in addChunks) InsertChunk(addChunk, chunkManager);
            Console.WriteLine("Finished adding chunks. Saving World Files.");
            World.Save();
            Console.WriteLine("Done.");
            return true;
        }

        public bool InsertChunk(GeoChunk addChunk, IChunkManager chunkManager)
        {
            ChunkRef newChunk;
            if (chunkManager.ChunkExists(addChunk.X, addChunk.Z)) { Console.WriteLine("Replacing existing chunk at X=" + addChunk.X + " Z=" + addChunk.Z); newChunk = chunkManager.GetChunkRef(addChunk.X, addChunk.Z); }
            else { Console.WriteLine("Adding new chunk at X=" + addChunk.X + " Z=" + addChunk.Z); newChunk = chunkManager.CreateChunk(addChunk.X, addChunk.Z); }
            // add the blocks
            AlphaBlockCollection newBlocks = newChunk.Blocks;
            newBlocks.AutoFluid = false;
            newBlocks.AutoLight = false;
            newBlocks.AutoTileTick = false;
            foreach (GeoBlock block in addChunk.Blocks)
            {
                newBlocks.SetBlock(block.X, block.Y, block.Z, new AlphaBlock(block.ID));
                newBlocks.SetData(block.X, block.Y, block.Z, block.Meta);
            }
            // add the biomes
            AnvilChunk chunk = newChunk.GetChunkRef() as AnvilChunk;
            TagNodeCompound level = chunk.Tree.Root["Level"] as TagNodeCompound;
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
            return true;
        }
    }
}
