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
            msg.AppendLine("add [Directory/Files*] - Add files for processing. Filename wildcards acceptable");
            msg.AppendLine("out [Directory] - Set output directory to merge data into");
            msg.AppendLine("list - Write to screen the current output and inputs");
            msg.AppendLine("stats - Shows number of regions and chunks in the set output directory");
            msg.AppendLine("run - Execute the merge process with the current output and inputs");
            msg.AppendLine("exit - Exits the GeoSharer Merge program");
            return msg.ToString();
        }
    }
}
