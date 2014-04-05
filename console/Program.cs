using System;
using System.Text;
using System.IO;
using net.azirale.geosharer.core;
using System.Collections.Generic;

namespace net.azirale.geosharer.console
{
    class Program
    {
        private static List<string> inputFiles = new List<string>();
        private static string outputDirectory = string.Empty;
        private static bool exiting = false;

        static void Main(string[] args)
        {
            Console.WriteLine(WelcomeMessage());
            Console.WriteLine(HelpMessage());
            do
            {
                Console.Write("> ");
                string commandText = Console.ReadLine();
                ProcessCommand(commandText);
            } while (!exiting);

            Console.WriteLine("Exiting GeoSharer...");
        }

        private static void ProcessCommand(string commandText)
        {
            string check = commandText.Split(' ')[0].ToLower();
            switch (check)
            {
                case "help":
                    CommandHelp();
                    break;
                case "add":
                    CommandAdd(commandText);
                    break;
                case "out":
                    CommandOut(commandText);
                    break;
                case "list":
                    CommandList();
                    break;
                case "reset":
                    CommandReset();
                    break;
                case "run":
                    CommandRun();
                    break;
                case "stats":
                    CommandStats();
                    break;
                case "exit":
                    CommandExit();
                    break;
                default:
                    Console.WriteLine(
                        "Not a recognised command '"
                        + check
                        + "'. Type 'help' to show a list of commands."
                        );
                    break;
            }
        }

        /// <summary>
        /// Writes the HelpMessage() to the console
        /// </summary>
        private static void CommandHelp()
        {
            Console.WriteLine(HelpMessage());
        }

        /// <summary>
        /// Add .geosharer files to be merged into the output directory. The program
        /// will use wildcard searches to catch multiple files in the given directory.
        /// </summary>
        /// <param name="commandText">Original console command given by the user</param>
        private static void CommandAdd(string commandText)
        {
            if (commandText.Length < 5)
            {
                Console.WriteLine("Specify an argument for 'add' command");
                return;
            }
            string arg = commandText.Substring(4); // post-'add '
            string expanded = Environment.ExpandEnvironmentVariables(arg);
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
                    Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(arg))
                    );
            }
            // Ditch if the directory cannot be found
            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Could not find directory '" + dirPath + "'");
                return;
            }
            Console.WriteLine("Adding '" + filePath + "' in '" + dirPath + "' ...");
            string[] files = Directory.GetFiles(dirPath, filePath);
            int added = 0;
            foreach (string s in files)
            {
                if (s.EndsWith(".geosharer"))
                {
                    ++added;
                    inputFiles.Add(s);
                }
            }
            Console.WriteLine("Added " + added + " .geosharer files.");
        }

        /// <summary>
        /// Set the output directory for the world save to merge chunk data into.
        /// </summary>
        /// <param name="commandText">Original console command given by the user</param>
        private static void CommandOut(string commandText)
        {
            if (commandText.Length < 5)
            {
                Console.WriteLine("Specify an argument for 'out' command");
                return;
            }
            string arg = commandText.Substring(4); // post-'out '
            if (!Directory.Exists(arg))
            {
                Console.WriteLine("Could not find directory '" + arg + "'");
            }
            else
            {
                Console.WriteLine("Output directory set.");
                outputDirectory = arg;
            }
        }

        /// <summary>
        /// Writes to the console the currently set output directory and list of set input files
        /// </summary>
        private static void CommandList()
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("Output Directory: ");
            msg.AppendLine(outputDirectory);
            msg.AppendLine("Input files...");
            foreach (string s in inputFiles)
            {
                msg.Append("    ");
                msg.AppendLine(s);
            }
            msg.AppendLine();
            Console.WriteLine(msg.ToString());
        }

        /// <summary>
        /// Resets GeoSharer to its starting conditions
        /// </summary>
        private static void CommandReset()
        {
            outputDirectory = string.Empty;
            inputFiles.Clear();
            Console.WriteLine("Input and Output settings reset");
        }

        /// <summary>
        /// Executes the merge procedure according to the current settings
        /// </summary>
        private static void CommandRun()
        {
            GeoMultifile gmf = new GeoMultifile();
            foreach (string s in inputFiles)
            {
                bool isOk = gmf.AttachFile(s);
                if (!isOk) Console.WriteLine("Input file '" + s + "' failed. Wrong version or filetype?");
            }
            // add progress indicators and whatnot
            List<GeoChunkRaw> rawData = gmf.GetLatestChunkData();
            // more progress indicators
            GeoWorldWriter gww = new GeoWorldWriter();
            gww.UpdateWorld(outputDirectory, rawData);
            Console.WriteLine("Run command complete.");
        }

        /// <summary>
        /// Indicates to the GeoSharer console program it should exit
        /// </summary>
        private static void CommandExit()
        {
            exiting = true;
        }

        /// <summary>
        /// Get the standard welcoming message for when the program executes
        /// </summary>
        /// <returns>String object containing the message</returns>
        private static string WelcomeMessage()
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine("========== GeoSharer Merge - Console Edition ===============");
            msg.Append("GeoSharer:");
            msg.Append(VersionInfo.Major);
            msg.Append('.');
            msg.Append(VersionInfo.Minor);
            msg.Append(" for Minecraft:");
            msg.Append(VersionInfo.MC);
            msg.Append(" Forge:");
            msg.Append(VersionInfo.Forge);
            msg.AppendLine();
            return msg.ToString();
        }

        /// <summary>
        /// Get the help message listing the available commands
        /// </summary>
        /// <returns>String object containing the message</returns>
        private static string HelpMessage()
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendLine("Commands available...");
            msg.AppendLine("help - Shows this list of commands");
            msg.AppendLine("add [Directory/Files*] - Add files for processing. Wildcards accetpable");
            msg.AppendLine("out [Directory] - Set output directory to merge data into");
            msg.AppendLine("list - Write to screen the current output and inputs");
            msg.AppendLine("run - Execute the merge process with the current output and inputs");
            msg.AppendLine("exit - Exits the GeoSharer Merge program");
            return msg.ToString();
        }

        /// <summary>
        /// Attempts to read the set output world and reports back the number of regions and chunks
        /// </summary>
        private static void CommandStats()
        {
            Substrate.AnvilWorld world = Substrate.AnvilWorld.Open(outputDirectory);
            if (world == null)
            {
                Console.WriteLine("Unable to open world at '" + outputDirectory + "'");
                return;
            }

            int regionCount = 0;
            int chunkCount = 0;
            foreach (Substrate.AnvilRegion r in world.GetRegionManager())
            {
                regionCount++;
                chunkCount += r.ChunkCount();
            }

            Console.WriteLine("Regions: " + regionCount);
            Console.WriteLine("Chunks: " + chunkCount);
        }
    }
}