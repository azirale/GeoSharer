using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace net.azirale.civcraft.GeoSharer
{
    class Program
    {
        static void Main(string[] args)
        {
            InteractiveConsole interactive = new InteractiveConsole();
            interactive.Activate();
            Console.WriteLine("Program ended.");
        }
    }

    class InteractiveConsole
    {


        // basic console features
        bool quitting = false;
        public void Activate()
        {
            while (!quitting)
            {
                ReadCommand();
            }
            Console.WriteLine("Leaving interactive mode");
        }

        private void ReadCommand()
        {
            Console.Write("> ");
            string input = Console.ReadLine();
            string[] args = CommandHelper.SplitCommandLine(input).ToArray();
            if (args.Length == 0) return;
            switch (args[0].ToLower())
            {
                case "output": SetOutputFolder(args[1]); return;
                case "input": SetInput(args[1]); return;
                case "run": Run(); return;
                case "test": Test(); return;
                case "quit": this.quitting = true; return;
                default: Console.WriteLine("Unrecognised command [" + args[0].ToLower() + "]"); return;
            }

            //switch (inputLower)
            //{
                
            //    case "quit": quitting = true; return;
            //    default: Console.WriteLine("Unknown command. Only commands active are 'test' and 'quit'"); return;
            //}
            // end in the switch, not here
        }

        private string output;
        private string[] input;

        private void SetOutputFolder(string outputPath)
        {
            output = outputPath;
            if (!Path.IsPathRooted(output)) output = Path.Combine(Directory.GetCurrentDirectory(), output);
            Console.WriteLine("Set output path to [" + output + "]");
        }

        private void SetInput(string fileSearch)
        {
            string dirPath = Path.GetDirectoryName(fileSearch);
            string filePath = fileSearch.Substring(dirPath.Length + 1);
            if (!Path.IsPathRooted(dirPath)) dirPath = Path.Combine(Directory.GetCurrentDirectory(), dirPath);
            input = Directory.GetFiles(dirPath, filePath);            
            Console.WriteLine("Set input files to the following...");
            foreach (string s in input)
            {
                Console.WriteLine(s);
            }
        }

        private void Run()
        {
            if (output == null)
            {
                Console.WriteLine("No output set");
                return;
            }
            if (input == null)
            {
                Console.WriteLine("No input set");
                return;
            }

            GeoReader reader = new GeoReader();
            GeoWorldBuilder builder = new GeoWorldBuilder();
            int i = 0;
            foreach (string s in input)
            {
                if (!reader.SetFile(s))
                {
                    Console.WriteLine("Could not read [" + s + "]");
                    continue;
                }
                i++;
                Console.WriteLine("Merging file " + i + " of " + input.Length);
                builder.CreateWorld(output, reader);
            }

        }

        private void Test()
        {

        }
    }

    static class CommandHelper
    {
        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
                              .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                              .Where(arg => !string.IsNullOrEmpty(arg));
        }

        public static IEnumerable<string> Split(this string str,
                                            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
    }
}