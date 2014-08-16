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

        #region Get Metadata
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
                List<GeoChunkMeta> returnValue;
                switch (version)
                {
                    case 4:
                        returnValue = GetChunkMetadataV4(zs, header);
                        break;
                    default:
                        throw new InvalidDataException("GeoSharer file '" + this.FilePath + "' has unrecognised version number '" + version + "'");
                }
                return returnValue;
            }
        }

        /// <summary>
        /// Read and return the chunk metadata from a geosharer file with format 4
        /// </summary>
        /// <param name="zs">Active GZipStream of the file</param>
        /// <param name="header">The 8 byte header already read from the GZipStream</param>
        /// <returns></returns>
        private List<GeoChunkMeta> GetChunkMetadataV4(GZipStream zs, byte[] header)
        {
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
        #endregion

        #region Get Data
        /// <summary>
        /// Read and return chunk data contained in the referenced geosharer file that matches a list of provided chunk metadata
        /// </summary>
        /// <param name="chunkMetadata">List of chunk metadata for which to get the actual chunk data from this geosharer file</param>
        /// <returns>A list of chunk data from the reference geosharer file</returns>
        public List<GeoChunkRaw> GetChunkData(List<GeoChunkMeta> chunkMetadata)
        {
            using (FileStream fs = File.OpenRead(FilePath))
            using (GZipStream zs = new GZipStream(fs, CompressionMode.Decompress))
            {
                chunkMetadata.Sort((x, y) => x.DataStart.CompareTo(y.DataStart));
                int nextBytePosition = 0;
                List<GeoChunkRaw> returnValue = new List<GeoChunkRaw>(chunkMetadata.Count);
                for (int i = 0; i < chunkMetadata.Count; ++i)
                {
                    if (chunkMetadata[i].SourcePath != this.FilePath) throw new ArgumentException("GeoFile.GetChunkData: Asked for chunk from a different file");
                    int jumpDistance = chunkMetadata[i].DataStart - nextBytePosition;
                    if (jumpDistance > 0)
                    {
                        zs.Advance(jumpDistance);
                        nextBytePosition += jumpDistance;
                    }
                    byte[] chunkdata;
                    if (chunkMetadata[i].DataEnd == -1)
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
                        int length = chunkMetadata[i].DataEnd - chunkMetadata[i].DataStart;
                        chunkdata = new byte[length];
                        zs.Read(chunkdata, 0, length);
                        nextBytePosition += length;
                    }
                    returnValue.Add(new GeoChunkRaw(chunkMetadata[i], chunkdata));
                }
                return returnValue;
            }
        }
        #endregion

        public static void WriteNew(string filePath, List<GeoChunkRaw> chunks)
        {
            if (!filePath.EndsWith(".geosharer")) filePath = filePath + ".geosharer";
            FileInfo fi = new FileInfo(filePath);
            using (FileStream fs = fi.Create())
            using (GZipStream zs = new GZipStream(fs, CompressionMode.Compress))
            {
                int version = 4;
                int chunkStartOffset = 4 + 4 + (4 + 4 + 8) * chunks.Count; // VERSION + NUMCHUNKS + ([numchunks]*X+Z+TIME+START)
                zs.Write(version.ToBigEndianByteArray());
                zs.Write(chunks.Count.ToBigEndianByteArray());
                for (int i = 0; i < chunks.Count; ++i) { zs.Write(chunks[i].Index.X.ToBigEndianByteArray()); }
                for (int i = 0; i < chunks.Count; ++i) { zs.Write(chunks[i].Index.Z.ToBigEndianByteArray()); }
                for (int i = 0; i < chunks.Count; ++i) { zs.Write(chunks[i].TimeStamp.ToBigEndianByteArray()); }
                for (int i = 0; i < chunks.Count; ++i)
                {
                    zs.Write(chunkStartOffset.ToBigEndianByteArray());
                    chunkStartOffset += chunks[i].Data.Length;
                }
                for (int i = 0; i < chunks.Count; ++i) { zs.Write(chunks[i].Data); }
                zs.Close();
                fs.Close();
            }
        }
    }
}
