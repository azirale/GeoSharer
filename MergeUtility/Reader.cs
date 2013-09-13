using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace net.azirale.civcraft.GeoSharer
{
    class GeoReader : IEnumerable<GeoChunk>
    {
        /***** CONSTRUCTOR MEMBERS **************************************************************/
        #region Constructor Members

        public GeoReader()
        {
            this.sourceFiles = new List<FileInfo>();
            this.totalLength = 0;
            this.currentPosition = 0;
        }

        #endregion

        private List<FileInfo> sourceFiles;
        private long totalLength;
        private long currentPosition;

        public bool AddFile(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                Console.WriteLine("ERROR: File does not exist: [" + filePath + "]");
                return false;
            }
            if (fi.Extension != ".geosharer")
            {
                Console.WriteLine("ERROR: Only [.geosharer] files may be used. Got [" + fi.Extension + "]");
                return false;
            }
            this.totalLength += fi.Length;
            this.sourceFiles.Add(fi);
            return true;
        }


        public string GetStatusLine()
        {
            if (this.totalLength == 0) return "Err-- no data to read;";
            StringBuilder builder = new StringBuilder();
            builder.Append(ShortenedSize(currentPosition));
            builder.Append('/');
            builder.Append(ShortenedSize(totalLength));
            builder.Append(" ~ ");
            builder.Append(((double)this.currentPosition / (double)this.totalLength).ToString("p"));
            return builder.ToString();
        }

        /// <summary>
        /// Returns the units suffix for memory amounts
        /// </summary>
        /// <param name="exponent">Exponent for 1024^E bytes</param>
        /// <returns></returns>
        private string GetUnit(int exponent)
        {
            switch (exponent)
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

        private string ShortenedSize(long size)
        {
            int exponent = 0;
            while (size >= 1024L)
            {
                size /= 1024;
                exponent++;
            }
            return size.ToString() + GetUnit(exponent);
        }

        #region Enumerator Implementation

        public IEnumerator<GeoChunk> GetEnumerator() { return new GeoReaderEnumerator(this); }
        IEnumerator IEnumerable.GetEnumerator() { return (IEnumerator)new GeoReaderEnumerator(this); }

        private class GeoReaderEnumerator : IEnumerator<GeoChunk>
        {

            public GeoReaderEnumerator(GeoReader reader)
            {
                this.parent = reader;
                this.fileNumber = -1;
                this.streamPos = 0;
                this.stream = null;
                this.zipStream = null;
                this.fileStream = null;
                this.current = null;
            }

            private GeoReader parent;
            private int fileNumber;
            private long streamPos;
            private StreamReader stream;
            private GZipStream zipStream;
            private FileStream fileStream;
            private GeoChunk current;

            public GeoChunk Current
            {
                get { return this.current; }
            }

            public void Dispose()
            {
                if (this.stream != null) this.stream.Dispose();
                if (this.zipStream != null) this.zipStream.Dispose();
                if (this.fileStream != null) this.fileStream.Dispose();
            }

            object IEnumerator.Current
            {
                get { return this.current; }
            }

            public bool MoveNext()
            {
                if (this.stream == null) if (!NextStream()) return false;
                if (this.stream.EndOfStream)
                {
                    if (!NextStream()) return false;
                }
                string text = this.stream.ReadLine();
                this.parent.currentPosition += this.fileStream.Position - this.streamPos;
                while (text == null) // this file was empty, keep going until we get a valid one or run out of files
                {
                    if (!NextStream()) return false;
                    text = this.stream.ReadLine();
                    this.parent.currentPosition += this.fileStream.Position - this.streamPos;
                }
                this.streamPos = this.fileStream.Position;
                this.current = GeoChunk.FromText(text);
                if (current == null) return false;
                return true;
            }

            private bool NextStream()
            {
                if (this.stream != null) this.stream.Dispose();
                if (this.zipStream != null) this.zipStream.Dispose();
                if (this.fileStream != null) this.fileStream.Dispose();
                this.fileNumber++;
                while (fileNumber < this.parent.sourceFiles.Count && !this.parent.sourceFiles[fileNumber].Exists) fileNumber++;
                if (this.fileNumber >= this.parent.sourceFiles.Count) return false; // Happens if the loop got to the end without finding a file
                try
                {
                    this.streamPos = 0;
                    this.fileStream = this.parent.sourceFiles[fileNumber].OpenRead();
                    this.zipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                    this.stream = new StreamReader(zipStream);
                }
                catch
                {
                    if (this.stream != null) this.stream.Dispose();
                    this.stream = null;
                    if (this.zipStream != null) this.zipStream.Dispose();
                    this.zipStream = null;
                    if (this.fileStream != null) this.fileStream.Dispose();
                    this.fileStream = null;
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
