using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WtTools.Formats.Extensions;

namespace WtTools.Formats.Vromfs
{
    internal class Header
    {
        internal string Magic { get; private set; }
        internal string Platform { get; private set; }
        internal uint OriginalSize { get; private set; }
        internal uint PackedSize { get; private set; }
        internal VromfsType VromfsType { get; private set; }
        internal VromfsPackageType PackageType { get; private set; }
        internal Header(BinaryReader reader)
        {
            Magic = reader.ReadBytes(4).ToUTF8String();
            Platform = reader.ReadBytes(4).ToUTF8String().Trim('\0');
            OriginalSize = reader.ReadUInt32();
            PackedSize = (uint)(reader.ReadUInt16() + (reader.ReadByte() << 16));
            VromfsType = (VromfsType)reader.ReadByte();
            switch (VromfsType)
            {
                case VromfsType.ZstdPacked:
                    PackageType = VromfsPackageType.ZstdPacked;
                    break;
                case VromfsType.MaybePacked:
                    if (PackedSize == 0)
                    {
                        PackageType = VromfsPackageType.NotPacked;
                    }
                    else
                    {
                        if (PackedSize > 0)
                        {
                            PackageType = VromfsPackageType.ZlibPacked;
                        }
                        else
                        {
                            PackageType = VromfsPackageType.Hoo;
                        }
                    }
                    break;
                default:
                    break;
            }

        }
    }
}
