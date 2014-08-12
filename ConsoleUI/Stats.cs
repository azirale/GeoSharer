using System;
using net.azirale.geosharer.core;

namespace net.azirale.geosharer.console
{
    class Stats
    {
        public static void Command(string ignored)
        {
            GeoWorldReader reader = new GeoWorldReader(WorldSelect.DirectoryFullPath);
            GeoWorldReader.StatsResults results = reader.GetWorldStats();
            Messaging.Send("Stats: Gathered stats for world '" + WorldSelect.DirectoryFullPath +"'");
            Messaging.Send("       Regions: " + results.NumberOfRegions);
            Messaging.Send("       Chunks:  " + results.NumberOfChunks);
        }
    }
}
