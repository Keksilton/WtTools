using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WtTools.Formats.Extensions;
using ZstdNet;

namespace WtTools.Formats
{
    internal struct NameMap
    {
        internal ulong Count;
        internal ulong Size;
        internal string[] Names;

        internal string this[int value]
        {
            get
            {
                return Names[value];
            }
        }

        internal string this[uint value]
        {
            get
            {
                return Names[value];
            }
        }

        public NameMap(byte[] data, DecompressionOptions decompOptions)
        {
            var nm = data[40..];
            using var nmStream = new MemoryStream(nm);
            using var nmDecompressionStream = new DecompressionStream(nmStream, decompOptions);
            var decompressedNm = nmDecompressionStream.ReadToEnd();
            using var decompressedNmStream = new MemoryStream(decompressedNm);
            using var reader = new BinaryReader(decompressedNmStream);
            Count = (ulong)reader.Read7BitEncodedInt64();
            Size = 0;
            Names = Array.Empty<string>();
            if (Count > 0)
            {
                Size = (ulong)reader.Read7BitEncodedInt64();
                var raw = reader.ReadBytes((int)Size);
                using var nmDataStream = new MemoryStream(raw);
                //result.Names = raw.GetUTF8String().Split("\0").ToArray();
                Names = new string[Count];
                for (ulong i = 0; i < Count; ++i)
                {
                    Names[i] = nmDataStream.ReadTerminatedString();
                }
            }
        }
    }
}
