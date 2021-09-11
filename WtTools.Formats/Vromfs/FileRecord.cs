using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Formats.Vromfs
{
    public struct FileRecord
    {
        public string Name;
        public int Size;
        public uint Offset;
        public byte[] Data;
    }
}
