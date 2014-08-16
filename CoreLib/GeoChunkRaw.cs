using Substrate;
using System;
using System.IO.Compression;
using System.IO;
using Substrate.Nbt;

namespace net.azirale.geosharer.core
{
    public class GeoChunkRaw : IChunkSync
    {
        private GeoChunkMeta meta;
        public byte[] Data { get; private set; }

        public GeoChunkRaw(GeoChunkMeta meta, byte[] data)
        {
            this.meta = meta;
            this.Data = data;
        }

        public GeoChunkRaw(NbtTree originalTree)
        {
            TagNodeCompound originalLevel = originalTree.Root["Level"].ToTagCompound();
            // get the metadata
            int x = originalLevel["xPos"].ToTagInt().Data;
            int z = originalLevel["zPos"].ToTagInt().Data;
            long timestamp = originalLevel.ContainsKey("GeoTimestamp") ? originalLevel["GeoTimestamp"].ToTagLong().Data : 0;
            this.meta = new GeoChunkMeta(x, z, timestamp, 0, 0, string.Empty);
            // get the data
            NbtTree newTree = new NbtTree();
            TagNodeCompound root = newTree.Root;
            TagNodeCompound level = new TagNodeCompound();
            root.Add("Level", level);
            level.Add("xPos", originalLevel["xPos"]);
            level.Add("zPos", originalLevel["zPos"]);
            level.Add("Biomes", originalLevel["Biomes"]);
            TagNodeList oldSections = originalLevel["Sections"] as TagNodeList;
            TagNodeList newSections = new TagNodeList(TagType.TAG_COMPOUND);
            foreach (TagNode node in oldSections)
            {
                TagNodeCompound oldSection = node as TagNodeCompound;
                TagNodeCompound newSection = new TagNodeCompound();
                newSections.Add(newSection);
                newSection.Add("Y", oldSection["Y"]);
                newSection.Add("Blocks", oldSection["Blocks"]);
                newSection.Add("Data", oldSection["Data"]);
                if (oldSection.ContainsKey("Add")) newSection.Add("Add", oldSection["Add"]);
            }
            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Compress))
            {
                newTree.WriteTo(zs);
                zs.Close();
                data = ms.ToArray();
            }
            this.Data = data;
        }

        public int X
        {
            get { return this.meta.X; }
        }

        public int Z
        {
            get { return this.meta.Z; }
        }

        public XZIndex Index
        {
            get { return this.meta.Index; }
        }

        public long TimeStamp
        {
            get { return this.meta.TimeStamp; }
        }

        public int RX { get { return (int)Math.Floor(this.X / 16D); } }
        public int RZ { get { return (int)Math.Floor(this.Z / 16D); } }

        public bool Equals(IChunkSync other)
        {
            return this.X == other.X && this.Z == other.Z;
        }

        public int CompareTo(IChunkSync other)
        {
            return this.TimeStamp.CompareTo(other.TimeStamp);
        }

        public AnvilChunk GetAnvilChunk()
        {
            try
            {
                
                AnvilChunk value = AnvilChunk.Create(GetTree());
                return value;
            }
            catch // invalid gzip header
            {
                return null;
            }

        }

        public TagNode GetTreeNode()
        {
            MemoryStream ms = new MemoryStream(this.Data);
            GZipStream zs = new GZipStream(ms, CompressionMode.Decompress);
            NbtTree tree = new NbtTree(zs);
            zs.Close();
            ms.Close();
            zs.Dispose();
            ms.Dispose();
            TagNodeCompound level = tree.Root as TagNodeCompound;
            TagNodeList sections = level["Sections"] as TagNodeList;
            foreach (TagNode node in sections)
            {
                TagNodeCompound section = node.ToTagCompound();
                section.Add("BlockLight", new TagNodeByteArray(new byte[2048]));
                section.Add("SkyLight", new TagNodeByteArray(new byte[2048]));
            }
            level.Add("HeightMap", new TagNodeIntArray(new int[256]));
            level.Add("Entities", new TagNodeList(TagType.TAG_BYTE));
            level.Add("TileEntities", new TagNodeList(TagType.TAG_BYTE));
            level.Add("TerrainPopulated", new TagNodeByte(1));
            level.Add("GeoTimestamp", new TagNodeLong(this.TimeStamp));

            // hacky solution added in 172 due to change in NBTTagCompound in minecraft
            TagNodeCompound root = new TagNodeCompound();
            root.Add("Level", level);
            return root as TagNode;
        }

        public NbtTree GetTree()
        {
            NbtTree hacktree = new NbtTree(GetTreeNode() as TagNodeCompound);
            return hacktree;
        }
    }
}
