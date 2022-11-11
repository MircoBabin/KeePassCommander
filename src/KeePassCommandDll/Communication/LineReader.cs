using System.IO;

namespace KeePassCommandDll.Communication
{
    public class LineReader
    {
        private StreamReader _streamReader;
        private StringReader _stringReader;

        public LineReader(StreamReader reader)
        {
            _streamReader = reader;
            _stringReader = null;
        }

        public LineReader(StringReader reader)
        {
            _streamReader = null;
            _stringReader = reader;
        }

        public string ReadLine()
        {
            if (_streamReader != null)
                return _streamReader.ReadLine();

            if (_stringReader != null)
                return _stringReader.ReadLine();

            return string.Empty;
        }
    }
}
