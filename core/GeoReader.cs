using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Reads multiple .geosharer files from the GeoSharer mod as an enumerable set of GeoChunk objects
    /// </summary>
    public class GeoReader : IEnumerable<GeoChunk>, IMessageSender
    {
        /***** CONSTRUCTOR MEMBERS **************************************************************/
        #region Constructor Members

        /// <summary>
        /// Create a new, empty, GeoReader object
        /// </summary>
        public GeoReader()
        {
            this.sourceFiles = new List<FileInfo>();
            this.TotalLength = 0;
            this.CurrentPosition = 0;
        }

        #endregion

        /***** PRIVATE FIELDS *******************************************************************/
        #region Private Fields
        /// <summary>
        /// The .geosharer files that have been attached to this reader
        /// </summary>
        private List<FileInfo> sourceFiles;
        #endregion

        /***** PUBLIC PROPERTIES (READONLY) *****************************************************/
        #region Public Properties (Readonly)
        /// <summary>
        /// The total byte length of all .geosharer files that have been attached to this reader
        /// </summary>
        public long TotalLength { get; private set; }

        /// <summary>
        /// The current byte position of the reader, relative to all attached .geosharer files
        /// </summary>
        public long CurrentPosition { get; private set; }
        #endregion


        /***** PUBLIC METHODS *******************************************************************/
        #region Public Methods
        /// <summary>
        /// Attach another .geosharer file to the reader
        /// </summary>
        /// <param name="filePath">Absolute or relative path to the .geosharer file to attach</param>
        /// <returns>TRUE for success, FALSE if failure</returns>
        public bool AttachFile(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                this.SendMessage(MessageVerbosity.Error, "ERROR: Could not find file '" + filePath + "'");
                return false;
            }
            if (fi.Extension != ".geosharer")
            {
                this.SendMessage(MessageVerbosity.Error,("ERROR: Only '*.geosharer' files may be used. Got [" + fi.Extension + "]"));
                return false;
            }
            bool alreadyExists = false;
            foreach (FileInfo existing in this.sourceFiles)
            {
                alreadyExists = alreadyExists | existing.FullName.Equals(fi.FullName);
            }
            if (alreadyExists)
            {
                this.SendMessage(MessageVerbosity.Normal, "Attempted to attach a second copy of the same file to a GeoReader");
                return false;
            }
            this.TotalLength += fi.Length;
            this.sourceFiles.Add(fi);
            this.sourceFiles.Sort(new FilesSorterDescending()); // sorted so we read the newest first
            return true;
        }

        private class FilesSorterDescending : IComparer<FileInfo>
        {
            public int Compare(FileInfo a, FileInfo b)
            {
                if (a.LastWriteTimeUtc < b.LastWriteTimeUtc) return 1;
                else if (a.LastWriteTimeUtc > b.LastWriteTimeUtc) return -1;
                else return 0;
            }
        }

        /// <summary>
        /// A useful text description of where the GeoReader object is up to 
        /// </summary>
        /// <returns></returns>
        public string GetStatusText()
        {
            if (this.TotalLength == 0) return "No data to read";
            StringBuilder builder = new StringBuilder();
            builder.Append(ShortenedSize(CurrentPosition));
            builder.Append('/');
            builder.Append(ShortenedSize(TotalLength));
            builder.Append(" ~ ");
            builder.Append(((double)this.CurrentPosition / (double)this.TotalLength).ToString("p"));
            return builder.ToString();
        }

        /// <summary>
        /// Reverse the byte order of a long value, converting between Java order and C#
        /// </summary>
        /// <param name="value">A long for which you need the value </param>
        /// <returns>Long </returns>
        private long Reverse(long value)
        {
            return Endian.Reverse(value);
        }

        private int Reverse(int value)
        {
            return Endian.Reverse(value);
        }

        /// <summary>
        /// Get the units suffix for memory amounts
        /// </summary>
        /// <param name="exponent">Exponent for 1024^E bytes</param>
        /// <returns>String representation of the units - eg B, KB, MB</returns>
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
        #endregion

        /***** IENUMERATOR IMPLEMENTATION *******************************************************/
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
                this.parent.CurrentPosition += this.fileStream.Position - this.streamPos;
                while (text == null) // this file was empty, keep going until we get a valid one or run out of files
                {
                    if (!NextStream()) return false;
                    text = this.stream.ReadLine();
                    this.parent.CurrentPosition += this.fileStream.Position - this.streamPos;
                }
                this.streamPos = this.fileStream.Position;
                this.current = GeoChunk.FromGeosharerText(text);
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

        /***** IMESSAGESENDER IMPLEMENTATION ****************************************************/
        #region IMessageSender Implementation
        /// <summary>
        /// IMessageSender event to send a message to a subscribing object method
        /// </summary>
        public event Message Messaging;

        /// <summary>
        /// IMessageSender method to get the list of object methods that are subscribed
        /// to the IMessageSender.Messaging event of this object
        /// </summary>
        /// <returns></returns>
        public Message GetMessagingList()
        {
            return this.Messaging;
        }

        /// <summary>
        /// Proc the IMessageSender.Messaging event of this object
        /// </summary>
        /// <param name="verbosity">The channel this message should be sent through</param>
        /// <param name="text">The text of this message</param>
        private void SendMessage(MessageVerbosity verbosity, string text)
        {
            Message msg = this.Messaging;
            if (msg != null) msg(this, new MessagePacket(verbosity, text));
        }
        #endregion
    }
}
