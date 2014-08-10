using Substrate;
using System;

namespace net.azirale.geosharer.console
{
    class Stats
    {
        public static void Command(string ignored)
        {
            AnvilWorld world = AnvilWorld.Open(WorldSelect.DirectoryFullPath);
            if (world == null)
            {
                Console.WriteLine("Stats: Unable to open world '" + WorldSelect.DirectoryFullPath + "'");
                return;
            }

            int regionCount = 0;
            int chunkCount = 0;
            foreach (AnvilRegion r in world.GetRegionManager())
            {
                regionCount++;
                chunkCount += r.ChunkCount();
            }
            Messaging.Send("Stats: Gathered stats for world '" + WorldSelect.DirectoryFullPath +"'");
            Messaging.Send("       Regions: " + regionCount);
            Messaging.Send("       Chunks:  " + chunkCount);
        }
    }
}
