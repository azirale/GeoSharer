using System.IO;

namespace net.azirale.geosharer.console
{
    static class WorldSelect
    {
        public static string DirectoryFullPath = string.Empty;

        public static void Command(string argumentText)
        {
            if (!Directory.Exists(argumentText))
            {
                Messaging.Send("Could not find directory '" + argumentText + "'");
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(argumentText);
                DirectoryFullPath = di.FullName;
                Messaging.Send("Output directory set to '" + DirectoryFullPath + "'");
            }
        }
    }
}
