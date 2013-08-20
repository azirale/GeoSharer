namespace net.azirale.civcraft.GeoSharer
{
    public class GeoBlock
    {
        private readonly GeoBlockVector position;
        private readonly GeoBlockData data;
        public int X { get { return position.X; } }
        public int Y { get { return position.Y; } }
        public int Z { get { return position.Z; } }
        public int ID { get { return data.ID; } }
        public int Meta { get { return data.Meta; } }
        public GeoBlock(GeoBlockVector position, GeoBlockData data)
        {
            this.position = position;
            this.data = data;
        }
    }
}
