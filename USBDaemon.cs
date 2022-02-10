using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace atriplex
{
    public class USBDaemon
    {
        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                List<char> LogicalDrives = DriveInfo.GetDrives().Select(x => x.Name.Remove(1)[0]).ToList();
                while (true)
                {
                    var drives = DriveInfo.GetDrives().ToList();
                    if (drives.Exists(x => x.VolumeLabel != "NMAP--USB"))
                    {
                        var nLD = drives.Select(x => x.Name.Remove(1)[0]).ToList();
                        if (LogicalDrives.Count < nLD.Count)
                        {
                            var distinct = nLD.Find(x => !LogicalDrives.Contains(x));
                            var path = distinct + ":\\";
                            Folder folder = new Folder(path);
                            GC.Collect();
                        }
                    }

                    Task.Delay(500).Wait();
                }
            });
        }
    }
}