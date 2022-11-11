using System.Collections.Generic;
using System.Text;

namespace KeePassCommand
{
    public class ProgramArguments
    {
        public string outfile = null;
        public Encoding outfile_encoding = null;

        public string filesystem = null;
        public bool namedpipe = false;

        public string outcommand = null;
        public List<string> outargs = new List<string>();
    }
}
