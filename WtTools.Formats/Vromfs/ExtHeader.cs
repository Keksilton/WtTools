using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Formats.Vromfs
{
    internal class ExtHeader
    {
        internal ushort Size { get; set; }
        internal ushort Flags { get; set; }
        internal uint Version { get; set; }
        internal ExtHeader(BinaryReader reader)
        {
            Size = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            Version = reader.ReadUInt32();
        }
    }
}
