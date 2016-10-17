using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption
{
    public static class StreamExtensions
    {
        public static void Append(this Stream stream, byte value, ulong count)
        {
            for(;count>0;count--)
                stream.Append(new[] { value });
        }

        public static void Append(this Stream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }
    }
}
