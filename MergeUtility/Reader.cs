using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net.azirale.civcraft.GeoSharer
{
    class Reader
    {
        public List<GeoChunk> GeoChunks { get; private set; }
        public string Report { get { return reportBuilder.ToString(); } }
        private StringBuilder reportBuilder;

        public Reader()
        {
            GeoChunks = new List<GeoChunk>();
            reportBuilder = new StringBuilder();
        }

        public bool ReadFile(string path)
        {
            // Check that the file is ok
            if (!path.EndsWith(".geosharer"))
            {
                Console.WriteLine("ERROR: Only [.geosharer] files may be used");
                return false;
            }
            if (!File.Exists(path))
            {
                Console.WriteLine("ERROR: No file at [" + path + "]");
                return false;
            }
            // Open a stream and read it into a list of string objects
            StreamReader stream = new StreamReader(path);
            List<string> chunksText = new List<string>();
            while (!stream.EndOfStream)
            {
                string newChunkText = stream.ReadLine().TrimEnd('\n');
                chunksText.Add(newChunkText);
            }
            // Parse the string objects into more meaningful chunk objects


            foreach (string s in chunksText)
            {
                byte[] decode = Convert.FromBase64String(s);
                MemoryStream inStream = new MemoryStream(decode);
                GZipStream zipper = new GZipStream(inStream, CompressionMode.Decompress);
                MemoryStream outStream = new MemoryStream();
                zipper.CopyTo(outStream);
                byte[] chunkData = outStream.ToArray();
                GeoChunk chunk = new GeoChunk();
                chunk.ParseByteArray(chunkData);
                GeoChunks.Add(chunk);
                reportBuilder.AppendLine("Got " + chunk.ToString());
            }
            return true;
        }
    }
}
