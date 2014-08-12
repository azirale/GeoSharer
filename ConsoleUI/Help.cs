using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.azirale.geosharer.console
{
    static class Help
    {
        /// <summary>
        /// Writes the HelpMessage() to the console
        /// </summary>
        public static void Command(string ignored)
        {
            Messaging.Send(GetMessageText());
        }

        /// <summary>
        /// Get the help message listing the available commands
        /// </summary>
        /// <returns>String object containing the message</returns>
        public static string GetMessageText()
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine("Commands available...");
            msg.AppendLine("help - Shows this list of commands");
            msg.AppendLine("input [Directory/Files*] - Set geosharer files for input");
            msg.AppendLine("world [Directory] - Sets the directory for the world save to work with");
            msg.AppendLine("stats - Shows number of regions and chunks in the set world save");
            msg.AppendLine("merge - Execute the merge process with the current output and inputs");
            msg.AppendLine("      - Has optional argument tags that can be combined");
            msg.AppendLine("        l : disables lighting updates (slightly faster)");
            msg.AppendLine("        f : disables fluid recalculation (slightly faster)");
            msg.AppendLine("        s : disables chunk stitching calculations (significantly faster)");
            msg.AppendLine("        Example: 'merge lfs'");
            msg.AppendLine("exit - Exits the GeoSharer Merge program");
            return msg.ToString();
        }
    }
}
