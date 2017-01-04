using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileEncryption.Models
{
    [Serializable]
    public class IHeaderDefinition
    {

    }

    [Serializable]
    public class HeaderDefinitionV01 : IHeaderDefinition
    {
        public IList<FolderHeaderV01> folders { get; set; }

        public HeaderDefinitionV01()
        {
            folders = new List<FolderHeaderV01>();
        }
    }
}
