using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeePassCommandDll.Communication
{
    public class ResponseItem
    {
        public List<string> Parts = new List<string>();

        public ResponseItem(string col)
        {
            Parts.Add(col);
        }

        public ResponseItem(string col1, string col2)
        {
            Parts.Add(col1);
            Parts.Add(col2);
        }
    }
}
