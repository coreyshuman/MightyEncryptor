using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption.Models
{
    public class FileHeader
    {
        public ulong filesize { get; set; }
        public string filename { get; set; } // shortened path and filename
        public string fullPath { get; set; } // full path to file
    }
}
