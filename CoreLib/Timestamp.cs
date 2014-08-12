using System;

namespace net.azirale.geosharer.core
{
    public struct Timestamp : IEquatable<Timestamp>
    {
        private long unixTime;
        private static DateTime UnixEpoch = new DateTime(1960, 1, 1);
        public Timestamp(long unixTimeInMilliseconds) { this.unixTime = unixTimeInMilliseconds; }
        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(BitConverter.IsLittleEndian ? this.unixTime : Endian.Reverse(this.unixTime));
        }

        public static Timestamp FromBytes(byte[] bytes, int offset)
        {
            long unixtime = BitConverter.IsLittleEndian ? BitConverter.ToInt64(bytes, offset) : Endian.Reverse(BitConverter.ToInt64(bytes, offset));
            Timestamp value = new Timestamp();
            value.unixTime = unixtime;
            return value;
        }


        #region Overrides
        public override bool Equals(object obj)
        {
            if (!(obj is Timestamp)) { return false; }
            return this.Equals((Timestamp)obj);
        }

        public bool Equals(Timestamp other)
        {
            return this.unixTime == other.unixTime;
        }

        public override int GetHashCode()
        {
            return this.unixTime.GetHashCode();
        }

        public override string ToString()
        {
            return UnixEpoch.AddMilliseconds(this.unixTime).ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion

        #region Comparison operators
        public static bool operator ==(Timestamp a, Timestamp b) { return a.unixTime == b.unixTime; }
        public static bool operator !=(Timestamp a, Timestamp b) { return a.unixTime != b.unixTime; }
        public static bool operator >(Timestamp a, Timestamp b) { return a.unixTime > b.unixTime; }
        public static bool operator <(Timestamp a, Timestamp b) { return a.unixTime < b.unixTime; }
        public static bool operator >=(Timestamp a, Timestamp b) { return a.unixTime >= b.unixTime; }
        public static bool operator <=(Timestamp a, Timestamp b) { return a.unixTime <= b.unixTime; }
        #endregion
    }
}
