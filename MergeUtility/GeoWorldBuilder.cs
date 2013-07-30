using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Substrate;
using Substrate.Core;
using System.IO;

namespace net.azirale.civcraft.GeoSharer
{
    class GeoWorldBuilder
    {
        private NbtWorld World;
        private Level Level;

        public bool CreateWorld(string folderPath, List<GeoChunk> addChunks)
        {
            if (!Directory.Exists(folderPath)) try { Directory.CreateDirectory(folderPath); }
                catch
                {
                    Console.WriteLine("Unable to create directory, aborting world creation");
                    return false;
                }
            World = AnvilWorld.Create(folderPath);
            Console.WriteLine("Trimming to unique chunks. Started with " + addChunks.Count + " chunks");
            addChunks.Sort();
            List<GeoChunk> uniqueChunks = addChunks.Distinct().ToList();
            Console.WriteLine("Trimmed to " + uniqueChunks.Count + " unique chunks");
            IChunkManager chunker = World.GetChunkManager();
            foreach (GeoChunk addChunk in uniqueChunks) InsertChunk(addChunk, chunker);

            foreach (ChunkRef chunk in chunker)
            {
                AlphaBlockCollection blocker = chunk.Blocks;
                blocker.RebuildHeightMap();
                blocker.RebuildBlockLight();
                blocker.RebuildSkyLight();
                chunker.SaveChunk(chunk);
            }
            return true;
        }

        public bool InsertChunk(GeoChunk addChunk, IChunkManager chunker)
        {
            ChunkRef newChunk;
            if (chunker.ChunkExists(addChunk.X, addChunk.Z)) { Console.WriteLine("Replacing existing chunk at X=" + addChunk.X + " Z=" + addChunk.Z); newChunk = chunker.GetChunkRef(addChunk.X, addChunk.Z); }
            else { Console.WriteLine("Adding new chunk at X=" + addChunk.X + " Z=" + addChunk.Z); newChunk = chunker.CreateChunk(addChunk.X, addChunk.Z); }
            AlphaBlockCollection newBlocks = newChunk.Blocks;
            newBlocks.AutoFluid = false;
            newBlocks.AutoLight = false;
            newBlocks.AutoTileTick = false;
            foreach (GeoBlock block in addChunk.Blocks)
            {
                newBlocks.SetBlock(block.X, block.Y, block.Z, new AlphaBlock(block.ID));
                newBlocks.SetData(block.X, block.Y, block.Z, block.Meta);
            }
            newChunk.IsDirty = true;
            newChunk.IsTerrainPopulated = true;
            chunker.SaveChunk(newChunk);
            return true;
        }

    }
}
