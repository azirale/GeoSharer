
namespace net.azirale.geosharer.core
{
    public class GeoChunkMeta : IChunkSync
    {
        private int x;
        private int z;
        private long timestamp;
        private int dataStart;
        private int dataEnd;
        private string sourcePath;

        public GeoChunkMeta(int x, int z, long timestamp, int dataStart, int dataEnd, string sourcePath)
        {
            this.x = x;
            this.z = z;
            this.timestamp = timestamp;
            this.dataStart = dataStart;
            this.dataEnd = dataEnd;
            this.sourcePath = sourcePath;
        }

        public int X
        {
            get { return this.x; }
        }

        public int Z
        {
            get { return this.z; }
        }

        public long TimeStamp
        {
            get { return this.timestamp; }
        }

        public int DataStart
        {
            get { return this.dataStart; }
        }

        public int DataEnd
        {
            get { return this.dataEnd; }
        }

        public string SourcePath
        {
            get { return this.sourcePath; }
        }

        public override bool Equals(object obj)
        {
            GeoChunkMeta other = obj as GeoChunkMeta;
            return (other != null && this.Equals(other));
        }

        public override int GetHashCode()
        {
            return 17 * this.x.GetHashCode() + this.z.GetHashCode();
        }

        public bool Equals(IChunkSync other)
        {
            return this.x == other.X && this.z == other.Z;
        }

        public int CompareTo(IChunkSync other)
        {
            return this.timestamp.CompareTo(other.TimeStamp);
        }
    }
}
