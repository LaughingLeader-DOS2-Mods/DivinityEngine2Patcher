using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
     
namespace PatchLoader
{
    class Program
    {
        private static string PluginsDirectory;
        private static string ExePath;

        static void Main(string[] args)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ExePath = Path.Combine(currentDirectory, "DivinityEngine2.exe");
            PluginsDirectory = Path.Combine(currentDirectory, "Patches");

            if(!File.Exists(ExePath))
			{
                Console.WriteLine($"Failed to fine DivinityEngine2.exe at {ExePath}");
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey(true);
                Environment.Exit(2);
            }
            Directory.CreateDirectory(PluginsDirectory);

            Loader.LoadPatches(ExePath, PluginsDirectory);
        }
    }
}