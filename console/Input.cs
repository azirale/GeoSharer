using System;
using System.Collections.Generic;
using System.IO;

namespace net.azirale.geosharer.console
{
    static class Input
    {
        public static List<string> FilePaths = new List<string>();

        public static void Command(string fileSearchPath)
        {
            string expanded = Environment.ExpandEnvironmentVariables(fileSearchPath);
            string filePath = Path.GetFileName(expanded);
            string dirPath;
            if (Path.IsPathRooted(expanded))
            {
                dirPath = expanded.Substring(0, expanded.Length - filePath.Length);
            }
            else
            {
                dirPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(fileSearchPath))
                    );
            }
            // Ditch if the directory cannot be found
            if (!Directory.Exists(dirPath))
            {
                Messaging.Send("Input: Could not find directory '" + dirPath + "'");
                return;
            }
            Messaging.Send("Input: Adding '" + filePath + "' in '" + dirPath + "' ...");
            string[] files = Directory.GetFiles(dirPath, filePath);
            int added = 0;
            foreach (string s in files)
            {
                if (s.EndsWith(".geosharer"))
                {
                    ++added;
                    FilePaths.Add(s);
                }
            }
            Console.WriteLine("Added " + added + " .geosharer files.");
        }
    }
}
