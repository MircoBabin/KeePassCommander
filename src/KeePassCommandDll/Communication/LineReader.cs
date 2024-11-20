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

        public LineReader(string data)
        {
            _streamReader = null;
            _stringReader = new StringReader(data);
        }

        public string ReadLine()
        {
            // StreamReader.ReadLine() + StringReader.ReadLine():
            //
            // A line is defined as a sequence of characters followed by a line feed ("\n"),
            // a carriage return ("\r"), or a carriage return immediately followed by a line feed ("\r\n").
            // The string that is returned does not contain the terminating carriage return or line feed.
            // The returned value is null if the end of the input stream is reached.

            if (_streamReader != null)
                return _streamReader.ReadLine();

            if (_stringReader != null)
                return _stringReader.ReadLine();

            return string.Empty;
        }
    }
}
