using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using Substrate;
using Substrate.Nbt;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Holds an index of timestamps
    /// </summary>
    class ChunkTimes
    {
        private Dictionary<ChunkIndex, Timestamp> timestamps = new Dictionary<ChunkIndex, Timestamp>();

        public void Reset()
        {
            this.timestamps.Clear();
        }

        public void WriteToFile(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            this.WriteToFile(file);
        }

        public void WriteToFile(FileInfo file)
        {
            if (!file.FullName.EndsWith(".geoindex")) new FileInfo(file.FullName + ".geoindex");
            using (FileStream fs = file.Create())
            using (GZipStream zs = new GZipStream(fs, CompressionMode.Compress))
            {
                foreach (KeyValuePair<ChunkIndex, Timestamp> kvp in this.timestamps)
                {
                    byte[] keyBytes = kvp.Key.ToBytes();
                    byte[] valueBytes = kvp.Value.ToBytes();
                    zs.Write(keyBytes, 0, keyBytes.Length);
                    zs.Write(valueBytes, 0, valueBytes.Length);
                }
            }
        }

        public static ChunkTimes FromFile(string filePath)
        {
            return FromFile(new FileInfo(filePath));
        }

        public static ChunkTimes FromFile(FileInfo file)
        {
            if (!file.FullName.EndsWith(".geoindex")) return null;
            if (!file.Exists) return null;
            ChunkTimes indexfile = new ChunkTimes();
            using (FileStream fs = file.OpenRead())
            using (GZipStream zs = new GZipStream(fs, CompressionMode.Decompress))
            {
                byte[] kvpBytes = new byte[16];
                zs.Read(kvpBytes, 0, 16);
                ChunkIndex index = ChunkIndex.FromBytes(kvpBytes, 0);
                Timestamp stamp = Timestamp.FromBytes(kvpBytes, 8);
                indexfile.timestamps[index] = stamp;
            }
            return indexfile;
        }

        public static ChunkTimes FromWorld(string worldPath)
        {
            AnvilWorld world = AnvilWorld.Open(worldPath);
            if (world == null) return null;
            ChunkTimes chunkIndices = new ChunkTimes();
            RegionChunkManager rcm = world.GetChunkManager();
            foreach (ChunkRef cr in rcm)
            {
                AnvilChunk ac = rcm.GetChunk(cr.X, cr.Z) as AnvilChunk;
                TagNodeCompound level = ac.Tree.Root["Level"] as TagNodeCompound;
                if (level.ContainsKey("GeoTimestamp"))
                {
                    ChunkIndex index = new ChunkIndex(cr.X, cr.Z);
                    Timestamp timestamp = new Timestamp(level["GeoTimestamp"].ToTagLong().Data);
                    chunkIndices.timestamps[index] = timestamp;
                }
            }
            return chunkIndices;
        }


        #region Methods by XZ values
        public void SetChunkTimestamp(int x, int z, long value)
        {
            this.timestamps[new ChunkIndex(x, z)] = new Timestamp(value);
        }

        public bool TryGetChunkTimestamp(int x, int z, out Timestamp value)
        {
            return this.timestamps.TryGetValue(new ChunkIndex(x, z), out value);
        }

        public Timestamp this[int x, int z]
        {
            get { return this.timestamps[new ChunkIndex(x, z)]; }
            set { this.timestamps[new ChunkIndex(x, z)] = value; }
        }
        #endregion

        #region Methods by ChunkIndex
        public void SetChunkTimestamp(ChunkIndex index, long value)
        {
            this.timestamps[index] = new Timestamp(value);
        }

        public bool TryGetChunkTimestamp(ChunkIndex index, out Timestamp value)
        {
            return this.timestamps.TryGetValue(index, out value);
        }

        public Timestamp this[ChunkIndex index]
        {
            get { return this.timestamps[index]; }
            set { this.timestamps[index] = value; }
        }
        #endregion
    }
}
