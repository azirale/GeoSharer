using System;
using net.azirale.geosharer.core;

namespace net.azirale.geosharer.console
{
    static class Recalculate
    {
        public static void Command(string options)
        {
            Console.CursorVisible = false;
            GeoWorldWriter gww = new GeoWorldWriter();
            gww.Progressing += Messaging.ReceiveProgress;
            Messaging.Send("Recalculate: Using options '" + options + "'");
            bool skipLighting = options.Contains("l");
            bool skipFluid = options.Contains("f");
            bool skipStitching = options.Contains("s");
            gww.RecalculateWorld(WorldSelect.DirectoryFullPath, !skipFluid, !skipLighting, !skipStitching);
            Messaging.Send("");
            Messaging.Send("Recalculate: Command complete.");
            Console.CursorVisible = true;
        }
    }
}
