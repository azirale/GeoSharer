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
                case "test":
                    CommandTest();
                    break;
                case "exit":
                case "quit":
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
            Console.CursorVisible = false;
            GeoMultifile gmf = new GeoMultifile();
            gmf.Messaging += ReceiveMessage;
            foreach (string s in inputFiles)
            {
                bool isOk = gmf.AttachFile(s);
                if (!isOk) Console.WriteLine("Input file '" + s + "' failed. Wrong version or filetype?");
            }
            List<GeoChunkRaw> rawData = gmf.GetLatestChunkData();
            GeoWorldWriter gww = new GeoWorldWriter();
            gww.Progressing += ReceiveProgress;
            gww.UpdateWorld(outputDirectory, rawData);
            Console.CursorTop++;
            Console.WriteLine("Added " + gww.Added + " chunks");
            Console.WriteLine("Updated " + gww.Updated + " chunks");
            Console.WriteLine("Run command complete.");
            Console.CursorVisible = true;
        }

        private static void CommandTest()
        {
            // pew pew
        }


        private static void CommandTest2()
        {
            Console.CursorVisible = false;
            GeoMultifile gmf = new GeoMultifile();
            gmf.Messaging += ReceiveMessage;
            foreach (string s in inputFiles)
            {
                bool isOk = gmf.AttachFile(s);
                if (!isOk) Console.WriteLine("Input file '" + s + "' failed. Wrong version or filetype?");
            }
            List<GeoChunkMeta> allMeta = gmf.GetChunkMetadata();
            System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
            clock.Start();
            List<GeoChunkMeta> newstyle = GetLatestChunkMeta(allMeta);
            clock.Stop();
            Console.WriteLine(clock.ElapsedMilliseconds + " ms");
            clock.Restart();
            List<GeoChunkMeta> oldstyle = GetLatestChunkMetaOld(allMeta);
            clock.Stop();
            Console.WriteLine(clock.ElapsedMilliseconds + " ms");

            newstyle.Sort();
            oldstyle.Sort();
            bool fail = false;

            Dictionary<long, GeoChunkMeta> dict = new Dictionary<long, GeoChunkMeta>();
            foreach (GeoChunkMeta each in oldstyle)
            {
                long index = ((long)(each.X) << 32) + (long)each.Z;
                if (dict.ContainsKey(index)) Console.WriteLine("Oldstyle has duplicate!");
                dict[index] = each;
            }
            foreach (GeoChunkMeta each in newstyle)
            {
                long index = ((long)(each.X) << 32) + (long)each.Z;
                if (!dict.ContainsKey(index)) Console.WriteLine("Newstyle has weird key!");
                else
                {
                    if (dict[index].TimeStamp != each.TimeStamp)
                    {
                        Console.WriteLine("Bad timestamp new=" + each.TimeStamp + " old=" + dict[index].TimeStamp);
                        fail = true;
                    }
                }
            }

            if (!fail) Console.WriteLine("SUCCESS");

            Console.CursorVisible = true;
        }
        public static List<GeoChunkMeta> GetLatestChunkMeta(List<GeoChunkMeta> allMeta)
        {
            Dictionary<long, GeoChunkMeta> dict = new Dictionary<long, GeoChunkMeta>();
            foreach (GeoChunkMeta each in allMeta)
            {
                long index = ((long)(each.X) << 32) + (long)each.Z;
                //Console.WriteLine("INDEX= 0x{0:x}", index);
                //Console.WriteLine("X= 0x{0:x}", each.X);
                //Console.WriteLine("Z= 0x{0:x}", each.Z);
                if (!dict.ContainsKey(index) || dict[index].TimeStamp < each.TimeStamp)
                {
                    //Console.WriteLine("New chunk X=" + each.X + " Z=" + each.Z);
                    if (dict.ContainsKey(index))
                    {
                        GeoChunkMeta existing = dict[index];
                        if (!existing.Equals(each)) Console.WriteLine("ERR X:" + each.X + "==" + existing.X + "? Z:" + each.Z + "==" + existing.Z + "?");
                    }
                    dict[index] = each;
                }/*
                else if (dict[index].TimeStamp<each.TimeStamp)
                {
                    Console.WriteLine("Updated chunk X=" + each.X + " Z=" + each.Z + " into X=" + dict[index].X + " Z=" + dict[index].Z);
                    dict[index] = each;
                }
                else
                {
                    Console.WriteLine("Discarded chunk X=" + each.X + " Z=" + each.Z + " for X=" + dict[index].X + " Z=" + dict[index].Z);
                    dict[index] = each;
                }*/
            }

            Console.WriteLine("Got " + allMeta.Count + " chunks");
            Console.WriteLine("Kept " + dict.Count + " chunks");

            List<GeoChunkMeta> value = value = new List<GeoChunkMeta>(dict.Values);
            return value;
        }

        public static List<GeoChunkMeta> GetLatestChunkMetaOld(List<GeoChunkMeta> allMeta)
        {
            allMeta.Sort();
            allMeta.Reverse();
            // filter to just the most recent chunks
            List<GeoChunkMeta> latestMeta = new List<GeoChunkMeta>();
            bool[] isLatest = new bool[allMeta.Count];
            System.Threading.CountdownEvent counter = new System.Threading.CountdownEvent(allMeta.Count);
            for (int i = 0; i < allMeta.Count; ++i)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CheckIsLatestMeta), new IsLatestMetaObject(counter, allMeta, isLatest, i));
            }
            counter.Wait();
            for (int i = 0; i < allMeta.Count; ++i)
            {
                if (isLatest[i]) latestMeta.Add(allMeta[i]);
            }
            Console.WriteLine("Got " + allMeta.Count + " chunks");
            Console.WriteLine("Kept " + latestMeta.Count + " chunks");
            return latestMeta;
        }
        private static void CheckIsLatestMeta(object arg)
        {
            IsLatestMetaObject value = arg as IsLatestMetaObject;
            GeoChunkMeta thisMeta = value.AllMeta[value.Position];
            for (int i = 0; i < value.AllMeta.Count; ++i)
            {
                GeoChunkMeta each = value.AllMeta[i];
                if (!thisMeta.Equals(each)) continue; // not the same chunk, ignore and continue
                if (value.Position == i) // According to sort we hit this chunk before any more recent one, therefore is most recent
                {
                    value.Flags[value.Position] = true;
                    value.Counter.Signal();
                    return;
                }
                if (thisMeta.TimeStamp < each.TimeStamp) // same chunk, but found one more recent than this
                {
                    value.Flags[value.Position] = false;
                    value.Counter.Signal();
                    return;
                }
            }
        }
        private class IsLatestMetaObject
        {
            public System.Threading.CountdownEvent Counter { get; private set; }
            public List<GeoChunkMeta> AllMeta { get; private set; }
            public bool[] Flags { get; private set; }
            public int Position { get; private set; }
            public IsLatestMetaObject(System.Threading.CountdownEvent counter, List<GeoChunkMeta> allMeta, bool[] flags, int position)
            {
                this.Counter = counter;
                this.AllMeta = allMeta;
                this.Flags = flags;
                this.Position = position;
            }
        }








        private static int cursorLeft = 0;
        private static int cursorTop = 0;
        private static DateTime lastUpdate = DateTime.Now;
        private static void ReceiveProgress(object sender, ProgressPacket progress)
        {
            //if (DateTime.UtcNow.AddSeconds(-1) < lastUpdate) return; // only once per second
            cursorLeft = Console.CursorLeft;
            cursorTop = Console.CursorTop;
            int barWidth = Console.BufferWidth - 2 - 7;
            double pctD = (double)progress.Current / (double)progress.Maximum;
            int pct = (int)((barWidth * progress.Current) / progress.Maximum);
            int remaining = pct == barWidth ? 0 : barWidth - pct;
            Console.Write(pctD.ToString("000.0% ") + '[' + new string('=', pct) + new string(' ', remaining) + ']');
            Console.CursorLeft = cursorLeft;
            Console.CursorTop = cursorTop;
        }

        public static void ReceiveMessage(object sender, MessagePacket msg)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss> ") + msg.Text);
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
            msg.AppendLine("add [Directory/Files*] - Add files for processing. Filename wildcards acceptable");
            msg.AppendLine("out [Directory] - Set output directory to merge data into");
            msg.AppendLine("list - Write to screen the current output and inputs");
            msg.AppendLine("stats - Shows number of regions and chunks in the set output directory");
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