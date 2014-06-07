using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace net.azirale.geosharer.core
{
    /// <summary>
    /// The GeoMultifile object is used to read multiple .geosharer files and serve up the
    /// chunk metadata and world data they contain. Use AttachFile() to add the .geosharer files,
    /// and then GetLatestChunkData() to get all the most recent world data for each chunk.
    /// </summary>
    public sealed class GeoMultifile : IProgressSender , IMessageSender
    {
        /***** CONSTRUCTOR MEMBERS **************************************************************/
        #region Constructor Members
        public GeoMultifile()
        {
            this.sourceFiles = new List<FileInfo>();
            this.chunksRequested = new List<GeoChunkMeta>();
        }
        #endregion

        /***** PRIVATE FIELDS *******************************************************************/
        #region Private Fields
        private List<FileInfo> sourceFiles;
        private List<GeoChunkMeta> chunksRequested;
        #endregion

        /***** PUBLIC METHODS *******************************************************************/
        #region Public Methods
        /// <summary>
        /// Attach another .geosharer file to this GeoMultifile object
        /// </summary>
        /// <param name="filePath">Path to the </param>
        /// <returns>False if a .geosharer is not found, True otherwise</returns>
        public bool AttachFile(string filePath)
        {
            if (!File.Exists(filePath) || !filePath.EndsWith(".geosharer")) return false;
            this.sourceFiles.Add(new FileInfo(filePath));
            return true;
        }

        /// <summary>
        /// Get all of the chunk metadata objects in the GeoMultifile Object. THIS METHOD IS MULTITHREADED
        /// </summary>
        /// <returns></returns>
        public List<GeoChunkMeta> GetChunkMetadata()
        {
            this.SendMessage(MessageVerbosity.Verbose, "Acquiring all chunk metadata");
            CountdownEvent counter = new CountdownEvent(this.sourceFiles.Count);
            List<List<GeoChunkMeta>> allMeta = new List<List<GeoChunkMeta>>(this.sourceFiles.Count);
            foreach (FileInfo fi in this.sourceFiles)
            {
                List<GeoChunkMeta> fileMeta = new List<GeoChunkMeta>();
                allMeta.Add(fileMeta);
                ThreadPool.QueueUserWorkItem(new WaitCallback(GetFileMeta), new GetFileMetaObject(fi, counter, fileMeta));
            }
            counter.Wait();
            List<GeoChunkMeta> value = new List<GeoChunkMeta>();
            foreach (List<GeoChunkMeta> eachMeta in allMeta)
            {
                value.AddRange(eachMeta);
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<GeoChunkMeta> GetLatestChunkMeta()
        {
            Dictionary<long, GeoChunkMeta> dict = new Dictionary<long, GeoChunkMeta>();
            List<GeoChunkMeta> allMeta = this.GetChunkMetadata();
            foreach (GeoChunkMeta each in allMeta)
            {
                long index = ((long)(each.X) << 32) + (long)each.Z;
                if (!dict.ContainsKey(index) || dict[index].TimeStamp < each.TimeStamp)
                {
                    dict[index] = each;
                }
            }
            this.SendMessage(MessageVerbosity.Normal, "Got " + allMeta.Count + " chunks");
            this.SendMessage(MessageVerbosity.Normal, "Kept " + dict.Count + " chunks");
            List<GeoChunkMeta> value = value = new List<GeoChunkMeta>(dict.Values);
            return value;
        }
        public int MetaCountTotal { get; private set; }
        public int MetaCountKept { get; private set; }

        
        /// <summary>
        /// Scans through all of the chunk metadata of the attached .geosharer files,
        /// then reads in only the most recent chunk world data from those files, and
        /// returns that data in a list.
        /// </summary>
        /// <returns>
        /// A List&lt;GeoChunkRaw&gt; containing the most recent world data for
        /// each chunk
        /// </returns>
        public List<GeoChunkRaw> GetLatestChunkData()
        {
            // Get only the most recent chunk data
            List<GeoChunkMeta> latestMeta = this.GetLatestChunkMeta();
            // We will return this
            List<GeoChunkRaw> value = new List<GeoChunkRaw>();
            this.SendMessage(MessageVerbosity.Verbose, "Getting data");
            // Scan each file - if it contains data we need: extract it then add it to the return
            CountdownEvent counter = new CountdownEvent(this.sourceFiles.Count);
            List<List<GeoChunkRaw>> allRaw = new List<List<GeoChunkRaw>>(this.sourceFiles.Count);
            foreach (FileInfo fi in this.sourceFiles)
            {
                List<GeoChunkRaw> fileRaw = new List<GeoChunkRaw>();
                allRaw.Add(fileRaw);
                ThreadPool.QueueUserWorkItem(new WaitCallback(GetLatestRaw), new GetLatestRawObject(counter, fi, fileRaw, latestMeta) );
            }
            counter.Wait();
            foreach(List<GeoChunkRaw> eachRaw in allRaw)
            {
                value.AddRange(eachRaw);
            }
            return value;
            // ORIGINAL
            /*
            foreach (FileInfo fi in this.sourceFiles)
            {
                List<GeoChunkMeta> thisFileMeta = new List<GeoChunkMeta>();
                for (int i = latestMeta.Count - 1; i >= 0; --i)
                {
                    if (latestMeta[i].SourcePath == fi.FullName)
                    {
                        thisFileMeta.Add(latestMeta[i]);
                        latestMeta.RemoveAt(i);
                    }
                }
                GeoFile gf = new GeoFile(fi.FullName);
                value.AddRange(gf.GetChunkData(thisFileMeta));
            }
            return value;
            */
        }

        private void GetLatestRaw(object arg)
        {
            GetLatestRawObject value = arg as GetLatestRawObject;
            if (value == null) throw new ArgumentException("GeoMultifile.GetLatestRaw did not get a GetLatestRawObject");

            List<GeoChunkMeta> thisFileMeta = new List<GeoChunkMeta>();
            for (int i = value.LatestMeta.Count - 1; i >= 0;--i)
            {
                if (value.LatestMeta[i].SourcePath == value.FI.FullName)
                {
                    thisFileMeta.Add(value.LatestMeta[i]);
                    //value.LatestMeta.RemoveAt(i);
                }
            }
            GeoFile gf = new GeoFile(value.FI.FullName);
            value.Value.AddRange(gf.GetChunkData(thisFileMeta));
            value.Counter.Signal();
        }

        private class GetLatestRawObject
        {
            public FileInfo FI { get; private set; }
            public CountdownEvent Counter { get; private set; }
            public List<GeoChunkRaw> Value { get; private set; }
            public List<GeoChunkMeta> LatestMeta { get; private set; }
            public GetLatestRawObject(CountdownEvent counter, FileInfo fi, List<GeoChunkRaw> value, List<GeoChunkMeta> latestMeta)
            {
                this.FI = fi;
                this.Counter = counter;
                this.Value = value;
                this.LatestMeta = latestMeta;
            }
        }


        #endregion


        /***** PRIVATE METHODS ******************************************************************/
        #region Private Methods
        /// <summary>
        /// Pulls all the chunk metadata from a single .geosharer file. Uses a generic object
        /// parameter to easy multithreading calls to this method
        /// </summary>
        /// <param name="o">GeoMultifile.GetFileMetaObject passed as a generic object</param>
        private void GetFileMeta(object o)
        {
            GetFileMetaObject args = o as GetFileMetaObject;
            if (args == null) throw new ArgumentException("GetFileMeta received an object that was not a GetFileMetaObject");
            GeoFile gf = new GeoFile(args.sourceFile.FullName);
            List<GeoChunkMeta> addme = gf.GetChunkMetadata();
            if (addme != null) args.meta.AddRange(addme);
            args.counter.Signal();
        }
        #endregion

        /***** NESTED CLASSES *******************************************************************/
        #region Nested Classes
        private class GetFileMetaObject
        {
            public GetFileMetaObject(FileInfo sourceFile, CountdownEvent counter, List<GeoChunkMeta> meta)
            {
                this.sourceFile = sourceFile;
                this.counter = counter;
                this.meta = meta;
            }
            public FileInfo sourceFile;
            public CountdownEvent counter;
            public List<GeoChunkMeta> meta;
        }
        #endregion

        public bool RequestChunk(GeoChunkMeta meta)
        {
            if (meta == null) return false;
            this.chunksRequested.Add(meta);
            return true;
        }

        /***** IPROGRESSSENDER IMPLEMENTATION ***************************************************/
        public event Progress Progressing;
        
        /***** IMESSAGESENDER IMPLEMENTATION ****************************************************/
        #region IMessageSender Implementation
        public event Message Messaging;

        public Message GetMessagingList()
        {
            return this.Messaging;
        }

        private void SendMessage(MessageVerbosity verbosity, string text)
        {
            Message msg = this.Messaging;
            if (msg != null) msg(this, new MessagePacket(MessageVerbosity.Error, text));
        }

        private void ErrorMessage(string text)
        {
            this.SendMessage(MessageVerbosity.Error, text);
        }
        #endregion
    }
}
