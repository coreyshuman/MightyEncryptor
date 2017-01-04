using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption.Models
{
    [Serializable]
    public class FileHeaderV01
    {
        public ulong filesize { get; set; }
        public string filename { get; set; } // shortened path and filename
        public string fullPath { get; set; } // full path to file

        public FileHeaderV01() { }
    }
}
