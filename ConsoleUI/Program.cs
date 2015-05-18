using System;
using System.Collections.Generic;

namespace net.azirale.geosharer.console
{
    class Program
    {
        #region Fields controlled by this class
        private static Dictionary<string, Command> CommandAliases;
        private static bool exiting;
        #endregion

        #region Constructor/Startup
        static void Main(string[] args)
        {
            PrepareCommands();
            exiting = false;
            if (args.Length > 0)
            {
                AutomatedMode(args);
            }
            else
            {
                InteractiveMode();
            }
            Messaging.Send("Exiting GeoSharer...");
        }

        private static void PrepareCommands()
        {
            CommandAliases = new Dictionary<string, Command>();
            AddCommandAliases(Help.Command, "help", "hepl", "h");
            AddCommandAliases(Input.Command, "input", "in", "i");
            AddCommandAliases(WorldSelect.Command, "world", "w");
            AddCommandAliases(Stats.Command, "stats", "s");
            AddCommandAliases(Merge.Command, "merge", "m");
            AddCommandAliases(Recalculate.Command, "recalculate", "recalc", "r");
            AddCommandAliases(CommandExit, "exit", "quit", "q", "x");
        }

        private static void AddCommandAliases(Command command, params string[] aliases)
        {
            foreach (string alias in aliases) CommandAliases.Add(alias, command);
        }
        #endregion

        #region Automated Mode
        /// <summary>
        /// Goes into an automated console mode that executes commands from the command line arguments used
        /// to open the program. The user will not be able to control the program.
        /// </summary>
        /// <param name="args">Arguments passed to the program from the command line</param>
        private static void AutomatedMode(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                string commandText = args[i].Substring(1);
                Command cmd;
                if (CommandAliases.TryGetValue(commandText, out cmd))
                {
                    if (i+1 < args.Length) { cmd(args[i+1]); } // this is incorrect, need to test if next argument is also a command
                    else cmd(string.Empty);
                }
                else Messaging.Send("Program: Did not recognise command switch '" + commandText + "'");
            }
        }
        #endregion

        #region Interactive Mode
        /// <summary>
        /// Goes into an interactive console mode where the user can enter specific commands
        /// and get help, responses, and feedback
        /// </summary>
        private static void InteractiveMode()
        {
            Messaging.Send(Help.GetMessageText());
            do
            {
                Console.Write("> ");
                string commandText = Console.ReadLine();
                ProcessCommand(commandText);
            } while (!exiting);
        }

        private static void ProcessCommand(string inputText)
        {
            string commandText = inputText.Split(' ')[0].ToLower();
            string argumentText = inputText.Length > commandText.Length ? inputText.Substring(commandText.Length + 1) : string.Empty;
            Command invoke;
            if (CommandAliases.TryGetValue(commandText, out invoke))
            {
                invoke(argumentText);
            }
            else
            {
                Messaging.Send(
                      "Not a recognised command '"
                      + commandText
                      + "'. Type 'help' to show a list of commands."
                      );
            }
        }
        #endregion

        /// <summary>
        /// Indicates to the GeoSharer console program it should exit
        /// </summary>
        private static void CommandExit(string ignored)
        {
            exiting = true;
        }

    }
}