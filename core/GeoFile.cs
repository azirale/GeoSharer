using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace net.azirale.geosharer.core
{
    class GeoFile
    {
        string filePath;

        public GeoFile(string filePath)
        {
            this.filePath = filePath;
        }

        public List<GeoChunkMeta> GetChunkMetadata()
        {
            if (!File.Exists(this.filePath)) return null;
            if (!this.filePath.EndsWith(".geosharer")) return null;
            FileStream fs = File.OpenRead(this.filePath);
            GZipStream zs = new GZipStream(fs, CompressionMode.Decompress);

            byte[] header = new byte[8];
            zs.Read(header, 0, 8);
            int version = Endian.Reverse(BitConverter.ToInt32(header, 0));
            if (version != 4) return null;
            int chunks = Endian.Reverse(BitConverter.ToInt32(header, 4));

            byte[] chunkMeta = new byte[chunks * (4 + 4 + 8 + 4)]; // int x, int y, long timestamp, int dataStart
            zs.Read(chunkMeta, 0, chunkMeta.Length);

            int xIndex = 0;
            int zIndex = 4*chunks;
            int timeIndex = zIndex+4*chunks;
            int startIndex = timeIndex + 8* chunks;

            List<GeoChunkMeta> value = new List<GeoChunkMeta>(chunks);
            for (int i = 0; i < chunks; ++i)
            {
                value.Add(new GeoChunkMeta(
                    Endian.Reverse(BitConverter.ToInt32(chunkMeta,xIndex+i*4)),
                    Endian.Reverse(BitConverter.ToInt32(chunkMeta,zIndex + i * 4)),
                    Endian.Reverse(BitConverter.ToInt64(chunkMeta,timeIndex + i * 8)),
                    Endian.Reverse(BitConverter.ToInt32(chunkMeta,startIndex + i * 4)),
                    i + 1 == chunks ? -1 : Endian.Reverse(BitConverter.ToInt32(chunkMeta,startIndex + i * 4 + 4)),
                    this.filePath));
            }
            zs.Close();
            fs.Close();
            return value;
        }

        public GeoChunkRaw[] GetChunkData(List<GeoChunkMeta> chunks)
        {
            if (!File.Exists(this.filePath)) return null;
            FileStream fs = File.OpenRead(filePath);
            GZipStream zs = new GZipStream(fs, CompressionMode.Decompress);
            chunks.Sort((x, y) => x.DataStart.CompareTo(y.DataStart));
            int nextBytePosition = 0;
            GeoChunkRaw[] value = new GeoChunkRaw[chunks.Count];
            for (int i = 0; i < chunks.Count;++i)
            {
                if (chunks[i].SourcePath != this.filePath) throw new ArgumentException("GeoFile.GetChunkData: Asked for chunk from a different file");
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
