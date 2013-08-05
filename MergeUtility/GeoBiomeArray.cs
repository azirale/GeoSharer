
namespace net.azirale.civcraft.GeoSharer
{
    public class GeoBiomeArray
    {
        public byte this[int x, int z] { get { return BiomeValues[x * 16 + z]; } }
        public byte[] CopyArray() { byte[] value = new byte[256]; for (int i = 0; i < 256; ++i) value[i] = BiomeValues[i]; return value; }
        private readonly byte[] BiomeValues;
        public GeoBiomeArray(byte[] biomeValues)
        {
            if (biomeValues.Length != 256) this.BiomeValues = null;
            else this.BiomeValues = biomeValues;
        }
    }
}
