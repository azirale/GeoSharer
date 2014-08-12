using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Represents a .geosharer file on the file system. Provides methods to read the chunk
    /// data and metadata contained in the file.
    /// </summary>
    class GeosharerFile
    {
        /// <summary>
        /// Immutable field, a GeosharerFile object only ever refers to a single file
        /// </summary>
        public readonly string FilePath;

        /// <summary>
        /// Create a new GeosharerFile object pointing to a particular file
        /// </summary>
        /// <param name="filePath">File system path to the geosharer file this object represents</param>
        public GeosharerFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new ArgumentException("New GeosharerFile received non-existant file '" + filePath + "'", "filePath");
            if (!filePath.EndsWith(".geosharer")) throw new ArgumentException("New GeosharerFile received a file that was not a '.geosharer' file: '" + filePath + "'", "filePath");
            this.FilePath = filePath;
        }

        /// <summary>
        /// Read and return the only the metadata from the beginning of the referenced geosharer file
        /// </summary>
        /// <returns>A list of the chunk metadata in the referenced geosharer file</returns>
        public List<GeoChunkMeta> GetChunkMetadata()
        {
            using (FileStream fs = File.OpenRead(this.FilePath))
            using (GZipStream zs = new GZipStream(fs, CompressionMode.Decompress))
            {
                byte[] header = new byte[8];
                zs.Read(header, 0, 8);
                int version = BitConverter.ToInt32(header, 0).EndianReverse();
                if (version != 4) return null; // check for valid version
                int numberOfChunks = BitConverter.ToInt32(header, 4).EndianReverse();
                byte[] chunkMetaBytes = new byte[numberOfChunks * (4 + 4 + 8 + 4)]; // int x, int y, long timestamp, int dataStart
                zs.Read(chunkMetaBytes, 0, chunkMetaBytes.Length);
                int xFirstByteIndex = 0; // Byte index where X values start
                int zFirstByteIndex = 4 * numberOfChunks; // Byte index where Z values start
                int timestampFirstByteIndex = zFirstByteIndex + 4 * numberOfChunks; // Byte index where timestamps start
                int dataindexFirstByteIndex = timestampFirstByteIndex + 8 * numberOfChunks; // Byte index where chunkstarts start
                List<GeoChunkMeta> returnValue = new List<GeoChunkMeta>(numberOfChunks);
                for (int i = 0; i < numberOfChunks; ++i)
                {
                    returnValue.Add(new GeoChunkMeta(
                        BitConverter.ToInt32(chunkMetaBytes, xFirstByteIndex + i * 4).EndianReverse(),
                        BitConverter.ToInt32(chunkMetaBytes, zFirstByteIndex + i * 4).EndianReverse(),
                        BitConverter.ToInt64(chunkMetaBytes, timestampFirstByteIndex + i * 8).EndianReverse(),
                        BitConverter.ToInt32(chunkMetaBytes, dataindexFirstByteIndex + i * 4).EndianReverse(),
                        i + 1 == numberOfChunks ? -1 : BitConverter.ToInt32(chunkMetaBytes, dataindexFirstByteIndex + i * 4 + 4).EndianReverse(),
                        this.FilePath));
                }
                return returnValue;
            }
        }

        public GeoChunkRaw[] GetChunkData(List<GeoChunkMeta> chunks)
        {
            if (!File.Exists(this.FilePath)) return null;
            FileStream fs = File.OpenRead(FilePath);
            GZipStream zs = new GZipStream(fs, CompressionMode.Decompress);
            
            chunks.Sort((x, y) => x.DataStart.CompareTo(y.DataStart));
            int nextBytePosition = 0;
            GeoChunkRaw[] value = new GeoChunkRaw[chunks.Count];
            for (int i = 0; i < chunks.Count;++i)
            {
                if (chunks[i].SourcePath != this.FilePath) throw new ArgumentException("GeoFile.GetChunkData: Asked for chunk from a different file");
                int jumpDistance = chunks[i].DataStart - nextBytePosition;
                if (jumpDistance > 0)
                {
                    Jump(zs, jumpDistance);
                    nextBytePosition += jumpDistance;
                }

                byte[] chunkdata;
                if (chunks[i].DataEnd == -1)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        zs.CopyTo(ms);
                        chunkdata = new byte[ms.Length];
                        ms.Read(chunkdata, 0, (int)ms.Length);
                    }
                }
                else
                {
                    int length = chunks[i].DataEnd - chunks[i].DataStart;
                    chunkdata = new byte[length];
                    zs.Read(chunkdata, 0, length);
                    nextBytePosition += length;
                }
                value[i] = new GeoChunkRaw(chunks[i], chunkdata);
            }
            zs.Dispose();
            fs.Dispose();
            return value;
        }

        private void Jump(GZipStream zs, int distance)
        {
            byte[] dump = new byte[distance];
            zs.Read(dump, 0, distance);
        }
    }
}
