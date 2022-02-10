using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using JGeneral;

namespace atriplex
{
    public ref struct Extract
    {
        public string Path
        {
            get;
            set;
        }

        public string AlternatePath
        {
            get
            {
                var path = $"{Platform.CurrentDirectory}\\codex_gigas\\{Path.Remove(0, 2)}";
                Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));
                return path;
            }
        }

        public Extract(string path)
        {
            Path = path;
            UnmanagedMemory = IntPtr.Zero;
            IsOpen = false;
        }

        public unsafe ReadOnlySpan<byte> GetData()
        {
            var fs = File.OpenRead(Path);
            IsOpen = true;
            var ptr = Marshal.AllocHGlobal((int) fs.Length);
            Span<byte> buffer = new Span<byte>(ptr.ToPointer(), (int)fs.Length);
            for (int i = 0; i < fs.Length; i++)
            {
                var b = fs.ReadByte();
                buffer[i] = (byte)b;
            }
            UnmanagedMemory = ptr;
            fs.Dispose();
            IsOpen = false;
            return buffer;
        }

        public long Length
        {
            get
            {
                if (!IsOpen)
                {
                    var fs = File.OpenRead(Path);
                    var len = fs.Length;
                    fs.Dispose();
                    return len;
                }
                else
                {
                    return 0;
                }
            }
        }

        private bool IsOpen;
        public IntPtr UnmanagedMemory;
        public const long MB = 1_000_000;
    }
}
