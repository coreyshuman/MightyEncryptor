using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption
{
    public class Debugging
    {
        private Form1 form;
        private static Debugging instance;

        private Debugging() { }

        public static Debugging Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Debugging();
                }
                return instance;
            }
        }

        public void SetForm(Form1 f1)
        {
            form = f1;
        }

        public void WriteLine(string line)
        {
            form?.AddDebugLine(line);
        }
    }
}
