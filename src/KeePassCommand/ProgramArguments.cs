using System.Collections.Generic;
using System.Text;

namespace KeePassCommand
{
    public class ProgramArguments
    {
        public enum StdoutEncodingType { Default, Utf8, Utf8WithoutBom } // Default encoding is set with "chcp" and is the OEM encoding.
        public StdoutEncodingType stdout_encoding = StdoutEncodingType.Default;

        public string outfile = null;
        public Encoding outfile_encoding = null;

        public string filesystem = null;
        public bool? namedpipe = null;

        public string outcommand = null;
        public List<string> outargs = new List<string>();
    }
}
