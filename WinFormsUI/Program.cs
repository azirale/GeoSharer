using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoSharerWinForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static Assembly LoadEmbeddedLibrary(object sender, ResolveEventArgs args)
        {
            // Assuming your 'program' class is in the default namespace
            string defaultNamespace = typeof(Program).Namespace;
            // The string here is the name of the folder in your project that the .dll files are listed under
            string embeddedLibrariesFolder = "EmbeddedLibraries";
            // the name of the dll file
            string dllName = new AssemblyName(args.Name).Name;
            // generate the full name to the resource in this assembly
            string resourceName = string.Format("{0}.{1}.{2}.dll", defaultNamespace, embeddedLibrariesFolder, dllName);
            // Load the assembly
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                Byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }
    }
}
