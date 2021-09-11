using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Formats.Extensions
{
    public static class StreamExtensions
    {
        public async static Task<byte[]> ReadToEndAsync(this BinaryReader reader)
        {
            using var ms = new MemoryStream();
            await reader.BaseStream.CopyToAsync(ms);
            return ms.ToArray();
        }
        public static byte[] ReadToEnd(this BinaryReader reader)
        {
            return reader.ReadToEndAsync().GetAwaiter().GetResult();
        }

        internal static NameMap ReadNameMap(this BinaryReader reader)
        {
            var result = new NameMap
            {
                Count = (ulong)reader.Read7BitEncodedInt64(),
            };
            if (result.Count > 0)
            {
                result.Size = (ulong)reader.Read7BitEncodedInt64();
                var raw = reader.ReadBytes((int)result.Size);
                using var nmStream = new MemoryStream(raw);
                //result.Names = raw.GetUTF8String().Split("\0").ToArray();
                result.Names = new string[result.Count];
                for (ulong i = 0; i < result.Count; ++i)
                {
                    result.Names[i] = nmStream.ReadTerminatedString();
                }
            }

            return result;
        }

        public static string ReadTerminatedString(this Stream reader)
        {
            using var ms = new MemoryStream();
            do
            {
                var b = reader.ReadByte();
                if (b <= 0)
                {
                    break;
                }
                ms.WriteByte((byte)b);
            } while (true);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            try
            {
                return sr.ReadToEnd();
            }
            catch
            {
                throw new NotImplementedException();
            }
        }

        public static byte[] ReadToEnd(this Stream stream)
        {
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
