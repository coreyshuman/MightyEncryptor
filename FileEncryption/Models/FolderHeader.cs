using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption.Models
{
    [Serializable]
    public class FolderHeaderV01
    {
        public string folderName { get; set; }
        public IList<FileHeaderV01> files { get; set; }

        public FolderHeaderV01()
        {
            files = new List<FileHeaderV01>();
        }
    }
}
