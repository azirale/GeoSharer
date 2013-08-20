using Substrate;
using Substrate.Core;
using Substrate.Nbt;
using System;
using System.Collections.Generic;

namespace net.azirale.civcraft.GeoSharer
{
    class Tester
    {
        public static void testme(GeoReader reader)
        {
            foreach (GeoChunk ch in reader)
            {
                for (int y = 255; y > 128; y--)
                {
                    //if (ch.Blocks.BlockAt(0, y, 0).ID != 0) Console.WriteLine("got non air: " + ch.Blocks.BlockAt(0, y, 0).ID.ToString());
                }
            }
        }
    }
}