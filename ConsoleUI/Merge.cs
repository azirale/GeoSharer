using net.azirale.geosharer.core;
using System;
using System.Collections.Generic;

namespace net.azirale.geosharer.console
{
    static class Merge
    {
        public static void Command(string options)
        {
            Console.CursorVisible = false;
            GeoMultifile gmf = new GeoMultifile();
            gmf.Messaging += Messaging.ReceiveMessage;
            foreach (string s in Input.FilePaths)
            {
                bool isOk = gmf.AttachFile(s);
                if (!isOk) Messaging.Send("Merge: File for input '" + s + "' failed. Wrong version or filetype?");
            }
            List<GeoChunkRaw> rawData = gmf.GetLatestChunkData();
            GeoWorldWriter gww = new GeoWorldWriter();
            gww.Progressing += Messaging.ReceiveProgress;
            Messaging.Send("Merge: Using options '" + options + "'");
            bool skipLighting = options.Contains("l");
            bool skipFluid = options.Contains("f");
            bool skipStitching = options.Contains("s");
            gww.UpdateWorld(WorldSelect.DirectoryFullPath, rawData, !skipFluid, !skipLighting, !skipStitching);
            Messaging.Send("");
            Messaging.Send("Merge: Added " + gww.Added + " chunks");
            Messaging.Send("Merge: Updated " + gww.Updated + " chunks");
            Messaging.Send("Merge: Command complete.");
            Console.CursorVisible = true;
        }
    }
}
