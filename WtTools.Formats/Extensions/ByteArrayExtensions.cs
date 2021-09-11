using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Formats.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToUTF8String(this byte[] data)
        {
            var result = System.Text.Encoding.UTF8.GetString(data);
            return result;
        }
    }
}
