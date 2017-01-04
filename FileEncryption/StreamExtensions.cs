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
        /// <summary>
        /// Copy a byte to the data stream 'count' times.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="count"></param>
        public static void Append(this Stream stream, byte value, ulong count)
        {
            for(;count>0;count--)
                stream.Append(new[] { value });
        }

        /// <summary>
        /// Append byte array to data stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="values"></param>
        public static void Append(this Stream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }
    }
}
