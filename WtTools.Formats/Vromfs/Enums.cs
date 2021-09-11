using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Formats.Vromfs
{
    internal enum VromfsPackageType
    {
        ZstdPacked,
        MaybePacked,
        ZlibPacked,
        NotPacked,
        Hoo
    }

    internal enum VromfsType
    {
        ZstdPacked = 0xc0,
        MaybePacked = 0x80,
        Unknown = 0x40
    }
}
