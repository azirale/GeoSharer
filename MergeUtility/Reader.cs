using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace net.azirale.civcraft.GeoSharer
{
    class GeoReader : IEnumerable<GeoChunk>, IEnumerator<GeoChunk>
    {
        private StreamReader stream;
        private GZipStream zipStream;
        private FileStream fileStream;
        private GeoChunk current;
        private FileInfo sourceFile;

        public long Position { get { if (fileStream == null) return 0; return fileStream.Position; } }
        public long Length { get { if (fileStream == null) return 0; return fileStream.Length; } }
        public string Status
        {
            get
            {
                if (this.Length == 0) return "Err-- no data to read";
                long current = this.Position;
                int cOrder = 0;
                while (current > 1024L) { current /= 1024; cOrder++; }
                long total = this.Length;
                int tOrder = 0;
                while (total > 1024L) { total /= 1024; tOrder++; }
                return (current + GetUnit(cOrder) + "/" + total + GetUnit(tOrder) + " ~ " + ((double)this.Position / (double)this.Length).ToString("p"));
            }
        }
        private string GetUnit(int order)
        {
            switch (order)
            {
                case 0: return "B";
                case 1: return "KB";
                case 2: return "MB";
                case 3: return "GB";
                case 4: return "TB";
                case 5: return "PB";
                default: return "!!";
            }
        }

        public GeoReader()
        {
            stream = null;
            fileStream = null;
            zipStream = null;
            current = null;
            sourceFile = null;
        }

        public bool SetFile(string path)
        {
            // Check that the file is ok
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            this.sourceFile = new FileInfo(path);
            if (!sourceFile.Exists)
            {
                Console.WriteLine("ERROR: No file at [" + path + "] for GeoReader");
                return false;
            }
            if (sourceFile.Extension != ".geosharer")
            {
                Console.WriteLine("ERROR: Only [.geosharer] files may be used. Got [" + sourceFile.Extension + "]");
                return false;
            }
            this.fileStream = this.sourceFile.OpenRead();
            this.zipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            this.stream = new StreamReader(zipStream);
            //this.stream = new StreamReader(sourceFile.FullName);
            return true;
        }

        # region Enumerator Implementation
        public GeoChunk Current
        {
            get { return current; }
        }

        public void Dispose()
        {
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream.Dispose();
                this.stream = null;
            }
        }

        object IEnumerator.Current
        {
            get { return current; }
        }

        public bool MoveNext()
        {
            if (this.stream == null) return false;
            if (stream.EndOfStream)
            {
                current = null;
                return false;
            }
            string text = stream.ReadLine();
            current = GeoChunk.FromText(text);
            if (current == null) return false;
            return true;
            
        }

        public void Reset()
        {
            current = null;
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
            stream = null;
        }

        // This object is its own enumerator
        public IEnumerator<GeoChunk> GetEnumerator() { return this; }

        // Pass the enumerator as the interface rather than the class
        IEnumerator IEnumerable.GetEnumerator() { return (IEnumerator)GetEnumerator(); }
        #endregion
    }
}
