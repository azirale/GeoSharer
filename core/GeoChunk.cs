using Substrate;
using Substrate.Nbt;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// Representation of a chunk from GeoSharer. Only holds the basic block data required to 
    /// build a full MC chunk from GeoSharer mod data, and does not work with lighting or
    /// entities. Has its own factory method to create object instances, and interfaces
    /// to assist with comparisons to other chunks.
    /// </summary>
    public class GeoChunk : IChunkSync, IEquatable<ChunkRef>, IMessageSender
    {
        /***** CONSTRUCTOR MEMBERS **************************************************/
        #region Constructor Members
        /// <summary>
        /// Cannot
        /// </summary>
        private GeoChunk() { }

        /// <summary>
        /// Factory method to create a GeoChunk from a line of text in a .geosharer file
        /// </summary>
        /// <param name="text">Line of text from a .geosharer file, such as from a GeoReader</param>
        /// <returns>A new GeoChunk, or null if the text could not be parsed correctly</returns>
        public static GeoChunk FromGeosharerText(string text)
        {
            return GeoChunk.FromGeosharerText(text, null);
        }

        /// <summary>
        /// Factory method to create a GeoChunk from a line of text in a .geosharer file, 
        /// and attaching a message receiver object for feedback, logging, and errors
        /// </summary>
        /// <param name="text">Line of text from a .geosharer file, such as from a GeoReader</param>
        /// <param name="receiver">An IMessageSender object, that will pass messages on from the parser</param>
        /// <returns>A new GeoChunk, or null if the text could not be parsed correctly</returns>
        public static GeoChunk FromGeosharerText(string text, IMessageSender receiver)
        {
            GeoChunk value = new GeoChunk();
            ByteArrayParser parser = new ByteArrayParser();
            if (receiver != null) parser.Messaging += receiver.GetMessagingList();
            bool valid = parser.Parse(value, text);
            if (!valid) return null;
            return value;
        }
        #endregion

        /***** PUBLIC READ / PRIVATE WRITE PROPERTIES *******************************/
        #region Public Read / Private Write Properties
        /// <summary>
        /// Unix timestamp in milliseconds of when the chunk was last updated
        /// </summary>
        public long TimeStamp { get; private set; }
        /// <summary>
        /// X coordinate of chunk within the world
        /// </summary>
        public int X { get; private set; }
        /// <summary>
        /// Z coordinate of chunk within the world
        /// </summary>
        public int Z { get; private set; }
        /// <summary>
        /// The highest Y layer of non-air blocks in this chunk
        /// </summary>
        public int MaxY { get; private set; }
        /// <summary>
        /// GeoSharer mod data transport version this chunk was built from
        /// </summary>
        public int Version { get; private set; }
        /// <summary>
        /// Biome data for each X/Z coordinate in the chunk
        /// </summary>
        public GeoBiomeArray Biomes { get; private set; }
        /// <summary>
        /// All of the blocks in this chunk
        /// </summary>
        public GeoBlockCollection Blocks { get; private set; }
        #endregion

        /***** PUBLIC READ PROPERTIES ***********************************************/
        #region Public Read Properties
        /// <summary>
        /// Get the TimeStamp of this chunk in a reading-friendly format
        /// </summary>
        public string TimeStampText
        {
            get
            {
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dt = dt.AddSeconds(this.TimeStamp / 1000).ToLocalTime();
                return dt.ToString("ddMMMyyyy-HH:mm:ss").ToUpper();
            }
        }
        #endregion

        /***** PUBLIC METHODS *******************************************************/
        #region Public Methods
        /// <summary>
        /// GeoChunk overrides the default ToString() implementation to provide more useful text
        /// </summary>
        /// <returns>A string describing the metadata aspects of the GeoChunk object</returns>
        public new string ToString()
        {
            return "GeoChunk: X[" + this.X.ToString() + "] Z[" + this.Z.ToString() + "] TimeStamp[" + this.TimeStampText + "]";
        }
        #endregion

        /***** INTERFACE IMPLEMENTATIONS ********************************************/
        #region Interface Implementations
        /// <summary>
        /// See if this GeoChunk has the same chunk X/Z coordinates as a Substrate.ChunkRef
        /// </summary>
        /// <param name="other">A Substrate.ChunkRef object</param>
        /// <returns>True if the X/Z chunk coordinates match, false otherwise</returns>
        public bool Equals(Substrate.ChunkRef other)
        {
            return this.X == other.X && this.Z == other.Z;
        }

        /// <summary>
        /// See if this GeoChunk object has the same X/Z coorinates as an object implementing the IChunkSync interface
        /// </summary>
        /// <param name="other">An object implementing the IChunkSync interface</param>
        /// <returns>True if the X/Z chunk coordinates match, false otherwise</returns>
        public bool Equals(IChunkSync other)
        {
            return this.X == other.X && this.Z == other.Z;
        }

        /// <summary>
        /// Make an ordinal comparison based on the timestamp
        /// </summary>
        /// <param name="other">An object implementing the IChunkSync interface</param>
        /// <returns>Int comparison value utilising Long.CompareTo()</returns>
        public int CompareTo(IChunkSync other)
        {
            return this.TimeStamp.CompareTo(other.TimeStamp);
        }

        /// <summary>
        /// Subscribe to this event to get status message from this object
        /// </summary>
        public event Message Messaging;

        /// <summary>
        /// Retrieve the subscribers to the messaging event of this object
        /// </summary>
        /// <returns></returns>
        public Message GetMessagingList()
        {
            return this.Messaging;
        }

        #endregion

        /***** BYTEARRAYPARSER NESTED CLASS *****************************************/
        #region ByteArrayParser Nested Class
        /// <summary>
        /// Class private to GeoChunk, this takes care of parsing the .geosharer data
        /// to fill a blank GeoChunk from GeoSharer mod data
        /// </summary>
        private class ByteArrayParser : IMessageSender
        {
            /***** PUBLIC METHODS *******************************************************/
            #region Public Methods
            /// <summary>
            /// Parse a text line from a geosharer file, to configure a GeoChunk object
            /// </summary>
            /// <param name="chunkOut">GeoChunk object to be modified</param>
            /// <param name="textIn">Text line from a .geosharer file</param>
            /// <returns>True if completed successfully, otherwise false</returns>
            public bool Parse(GeoChunk chunkOut, string textIn)
            {
                return this.NBTDecode(chunkOut, textIn);


                // Convert the text to a byte array
                byte[] bytes = TextToBytes(textIn);
                if (bytes == null)
                {
                    // spit error message
                    return false;
                }
                // parse the byte array into a chunk
                int version = bytes[0];
                switch (version)
                {
                    case 1:
                    case 2:
                        return this.Version1(chunkOut,bytes);
                    default:
                        throw new NotImplementedException("Invalid version code for .geosharer chunk");
                }
            }
            #endregion

            /***** PRIVATE METHODS ******************************************************/
            #region Private Methods
            /// <summary>
            /// Converts the original text line from the .geosharer file into the original byte array
            /// </summary>
            /// <param name="text">Text line from a .geosharer file</param>
            /// <returns>Byte array of the original data</returns>
            private byte[] TextToBytes(string text)
            {
                // decode, decompress, return
                try
                {
                    byte[] decode = Convert.FromBase64String(text);
                    MemoryStream inStream = new MemoryStream(decode);
                    GZipStream zipper = new GZipStream(inStream, CompressionMode.Decompress);
                    MemoryStream outStream = new MemoryStream();
                    zipper.CopyTo(outStream);
                    byte[] value = outStream.ToArray();
                    return value;
                }
                catch (Exception ex)
                {
                    this.SendMessage(MessageVerbosity.Error, "Unable to decode textline. Inner error '" + ex.Message + "'");
                    return null;
                }
            }

            private bool NBTDecode(GeoChunk parent, string text)
            {
                
                byte[] decode = Convert.FromBase64String(text);
                MemoryStream inStream = new MemoryStream(decode);
                GZipStream unzip = new GZipStream(inStream,CompressionMode.Decompress);
                NbtTree tree = new NbtTree(unzip);
                TagNodeCompound root = tree.Root;
                bool success = true;
                int version, x, z, maxY;
                long timestamp;
                byte[] biomes, blockIDs, blockData;
                success = success & TryGetInt(root, "Version", out version);
                success = success & TryGetInt(root, "X", out x);
                success = success & TryGetInt(root, "Z", out z);
                success = success & TryGetInt(root, "MaxY", out maxY);
                success = success & TryGetLong(root, "GeoTimestamp", out timestamp);
                success = success & TryGetByteArray(root, "Biomes", out biomes);
                success = success & TryGetByteArray(root, "BlockIDs", out blockIDs);
                success = success & TryGetByteArray(root, "BlockData", out blockData);
                if (!success)
                {
                    this.SendMessage(MessageVerbosity.Normal, "Got invalid GeoChunk NBT");
                    return false;
                }
                parent.Version = version;
                parent.X = x;
                parent.Z = z;
                parent.MaxY = maxY;
                parent.TimeStamp = timestamp;
                parent.Biomes = new GeoBiomeArray(biomes);
                parent.Blocks = new GeoBlockCollection(blockIDs, blockData, maxY);
                this.SendMessage(MessageVerbosity.Verbose, "Got GeoChunk " + parent.ToString());
                return true;
            }

            private bool TryGetByte(TagNodeCompound root, string key, out byte data)
            {
                TagNode node;
                if (!root.TryGetValue(key, out node) || node.GetTagType() != TagType.TAG_BYTE)
                {
                    data = 0;
                    return false;
                }
                data = node.ToTagByte().Data;
                return true;
            }

            private bool TryGetByteArray(TagNodeCompound root, string key, out byte[] data)
            {
                TagNode node;
                if (!root.TryGetValue(key, out node) || node.GetTagType() != TagType.TAG_BYTE_ARRAY)
                {
                    data = null;
                    return false;
                }
                data = node.ToTagByteArray().Data;
                return true;
            }

            private bool TryGetLong(TagNodeCompound root, string key, out long data)
            {
                TagNode node;
                if (!root.TryGetValue(key, out node) || node.GetTagType() != TagType.TAG_LONG)
                {
                    data = 0L;
                    return false;
                }
                data = node.ToTagLong().Data;
                return true;
            }

            private bool TryGetInt(TagNodeCompound root, string key, out int data)
            {
                TagNode node;
                if (!root.TryGetValue(key, out node) || node.GetTagType() != TagType.TAG_INT)
                {
                    data = 0;
                    return false;
                }
                data = node.ToTagInt().Data;
                return true;
            }

            /// <summary>
            /// Private method to parse .geosharer files created to version 1 specifications
            /// </summary>
            /// <param name="parent">The GeoChunk object to write out to</param>
            /// <param name="bytes">Byte array of the original data form the GeoSharer mod</param>
            /// <returns>True if the process succeeded, false if it failed</returns>
            private bool Version1(GeoChunk parent, byte[] bytes)
            {
                try
                {
                    int offset = 0;
                    // Version of saved geosharer chunk from mod
                    byte version = bytes[offset];
                    parent.Version = version;
                    offset++;
                    // time stamp for when chunk was stored
                    parent.TimeStamp = GetLong(bytes, offset);
                    offset += 8;
                    // chunk X position
                    parent.X = GetInt(bytes, offset);
                    offset += 4;
                    // chunk Z position
                    parent.Z = GetInt(bytes, offset);
                    offset += 4;
                    // biome data
                    byte[] biomeBytes = new byte[256];
                    Array.Copy(bytes, offset, biomeBytes, 0, 256);
                    parent.Biomes = new GeoBiomeArray(biomeBytes);
                    offset += biomeBytes.Length;
                    byte maxY = bytes[offset];
                    parent.MaxY = maxY;
                    offset++;
                    // block ids
                    byte[] idBytes = new byte[16 * 16 * (maxY + 1)];
                    Array.Copy(bytes, offset, idBytes, 0, idBytes.Length);
                    offset += idBytes.Length;
                    // block data
                    byte[] dataBytes = new byte[8 * 16 * (maxY + 1)];
                    Array.Copy(bytes, offset, dataBytes, 0, 8 * 16 * (maxY + 1));
                    offset += dataBytes.Length;
                    // create the new blocks
                    parent.Blocks = new GeoBlockCollection(idBytes, dataBytes, maxY);
                }
                catch (Exception ex)
                {
                    this.SendMessage(MessageVerbosity.Error, "Invalid byte array for new GeoChunk object: " + ex.Message);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Converts 8 bytes from a byte array into a long, reversing endianness because java
            /// </summary>
            /// <param name="bytes">Byte array from a .geosharer file</param>
            /// <param name="position">Position within the byte array of the first byte of the long</param>
            /// <returns>A Long value</returns>
            private long GetLong(byte[] bytes, int position)
            {
                long value = BitConverter.ToInt64(bytes, position);
                return IPAddress.HostToNetworkOrder(value);
            }

            /// <summary>
            /// Converts 4 bytes from a byte array into an int, reversing endianness because java
            /// </summary>
            /// <param name="bytes">Byte array from a .geosharer file</param>
            /// <param name="position">Position within the byte array of the first byte of the int</param>
            /// <returns>An Int value</returns>
            private int GetInt(byte[] bytes, int position)
            {
                int value = BitConverter.ToInt32(bytes, position);
                return IPAddress.HostToNetworkOrder(value);
            }
            #endregion


            /***** IMESSAGESENDER IMPLEMENTATION ****************************************/
            #region IMesageSender Implementation
            /// <summary>
            /// Fires the Messaging event from IMessageSender interface
            /// </summary>
            /// <param name="verbosity">The channel this message should be sent through</param>
            /// <param name="text">The text of the message</param>
            private void SendMessage(MessageVerbosity verbosity, string text)
            {
                Message msg = this.Messaging;
                if (msg != null) this.Messaging(this, new MessagePacket(verbosity, text));
            }
            /// <summary>
            /// Event fires when the parser wants to send a message up towards the user
            /// </summary>
            public event Message Messaging;
            /// <summary>
            /// Returns the list of subscribers to the Messaging event of this parser
            /// </summary>
            /// <returns></returns>
            public Message GetMessagingList()
            {
                return this.Messaging;
            }
            #endregion
        }
        #endregion

    }
}
