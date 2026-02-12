using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace ConsoleExecutor
{
    public class ConsoleExecutor : IDisposable
    {
        private string _enterPasswordText;
        private SecureString _password;
        private string _executable;
        private string _commandline;
        private string _workingdirectory;

        private IList<string> _appendToPath;
        private IDictionary<string, string> _environmentVariables;

        public enum ConsoleStatus : byte
        {
            Constructing,
            ThreadStarting,
            ThreadStarted,
            ProcessStarting,
            ProcessStarted,
            WaitForPasswordEnteringLock,
            PasswordEntering,
            PasswordEntered,
        }

        private object lockobj = new Object();
        private bool _threadabort = false;
        private Thread _thread = null;
        private ConsoleStatus _status = ConsoleStatus.Constructing;
        private ConsoleResult _result = ConsoleResult.Busy;
        private Exception _resultex = null;
        private int _resultExitCode = -1;
        private Process _process = null;
        private StringBuilder _output = new StringBuilder();
        private StringBuilder _error = new StringBuilder();

        public ConsoleStatus Status
        {
            get
            {
                lock (lockobj) { return _status; }
            }

            private set
            {
                lock (lockobj) { _status = value; }
            }
        }

        public enum ConsoleResult : byte
        {
            Busy,
            Exited,
            Exception,
            Disposed,
            Aborted
        }

        public bool IsDone()
        {
            ConsoleResult result = Result;

            return (result != ConsoleResult.Busy);
        }

        public ConsoleResult Result
        {
            get
            {
                lock (lockobj) { return _result; }
            }

            private set
            {
                lock (lockobj) { _result = value; }
            }
        }

        public int ExitCode
        {
            get
            {
                lock (lockobj) { return _resultExitCode; }
            }

            private set
            {
                lock (lockobj) { _resultExitCode = value; }
            }
        }

        public Exception ResultException
        {
            get
            {
                lock (lockobj) { return _resultex; }
            }

            private set
            {
                lock (lockobj) { _resultex = value; }
            }
        }

        public void ClearOutput()
        {
            lock (lockobj)
            {
                _output.Length = 0;
            }
        }

        public string Output
        {
            get
            {
                lock (lockobj) { return _output.ToString(); }
            }
        }

        public void ClearError()
        {
            lock (lockobj)
            {
                _error.Length = 0;
            }
        }

        public string Error
        {
            get
            {
                lock (lockobj) { return _error.ToString(); }
            }
        }

        public int ProcessId
        {
            get
            {
                return (_process != null ? _process.Id : 0);
            }
        }

        public void ClearInput()
        {
            lock (_inputQueue)
            {
                _inputQueue.Clear();
                _inputString.Length = 0;
            }
        }

        public string Input
        {
            get
            {
                lock (_inputQueue)
                {
                    return _inputString.ToString();
                }
            }
        }

        private enum _closeState
        {
            None,
            Close,
            Closed
        }

        private Queue<string> _inputQueue = new Queue<string>();
        private _closeState _inputQueueClose = _closeState.None;
        private StringBuilder _inputString = new StringBuilder();
        public void WriteToStandardInput(string text)
        {
            if (String.IsNullOrEmpty(text)) return;

            lock (_inputQueue)
            {
                if (_inputQueueClose != _closeState.None)
                    throw new Exception("Input queue is closed");

                _inputQueue.Enqueue(text);
                _inputString.Append(text);
            }
        }
        public void WriteLineToStandardInput(string text)
        {
            lock (_inputQueue)
            {
                if (_inputQueueClose != _closeState.None)
                    throw new Exception("Input queue is closed");

                if (!String.IsNullOrEmpty(text))
                {
                    _inputQueue.Enqueue(text);
                    _inputString.Append(text);
                }

                _inputQueue.Enqueue(Environment.NewLine);
                _inputString.Append(Environment.NewLine);
            }
        }
        public void CloseStandardInput()
        {
            lock (_inputQueue)
            {
                if (_inputQueueClose == _closeState.None)
                    _inputQueueClose = _closeState.Close;
            }
        }

        public static string EscapeParmsToCommandline(IList<string> parms)
        {
            if (parms == null)
                return string.Empty;

            /*
            https://learn.microsoft.com/en-us/dotnet/api/system.environment.getcommandlineargs?view=net-9.0&redirectedfrom=MSDN#System_Environment_GetCommandLineArgs

            Command line arguments are delimited by spaces. You can use double quotation marks (") to include spaces
            within an argument. The single quotation mark ('), however, does not provide this functionality.

            If a double quotation mark follows two or an even number of backslashes,
            each proceeding backslash pair is replaced with one backslash and the double quotation mark is removed.

            If a double quotation mark follows an odd number of backslashes, including just one,
            each preceding pair is replaced with one backslash and the remaining backslash is removed;
            however, in this case the double quotation mark is not removed.

            Original                                      Escaped
                                                          ""
            .                                             "."
            \path                                         "\path"
            \path\                                        "\path\\"
            "\path\"                                      "\"\path\\\""

            ----------------------------------------------------------------------------------
            var testvectors = new string[] { "", ".", "\\", "\"", "test", "\\test\\", "\"test\"", "\\\"test\\\"", "te\"st" };
            foreach (var vector in testvectors)
            {
                Console.WriteLine(vector);
                Console.WriteLine(EscapeParmsToCommandline(new string[] { vector }));
                Console.WriteLine();
            }

            Console.WriteLine(EscapeParmsToCommandline(testvectors));
            ----------------------------------------------------------------------------------
            */
            StringBuilder cmdline = new StringBuilder();
            foreach (string argument in parms)
            {
                if (argument != null)
                {
                    cmdline.Append("\"");

                    // find trailing backslashes
                    int p4 = argument.Length - 1;
                    while (p4 >= 0 && argument[p4] == '\\') p4--;
                    // p4 == -1    -->    argument consisting of only backslashes or empty string

                    // replace " inside argument
                    int p1 = 0;
                    while (p1 <= p4)
                    {
                        // find "
                        int p3 = argument.IndexOf('"', p1);
                        if (p3 < 0)
                        {
                            cmdline.Append(argument.Substring(p1, p4 - p1 + 1));
                            break;
                        }

                        // backslashes before the "
                        int p2 = p3 - 1;
                        while (p2 >= 0 && argument[p2] == '\\') p2--;
                        // p2 == -1    -->    \" at the beginning of parm

                        // append part before the backslashes and "
                        cmdline.Append(argument.Substring(p1, p2 - p1 + 1));

                        // double the number of preceding backslashes
                        cmdline.Append(new string('\\', 2 * (p3 - (p2 + 1))));

                        // append \" (odd number of backslashes preceding the quotation mark)
                        cmdline.Append("\\\"");

                        // next search
                        p1 = p3 + 1;
                    }

                    // double the number of trailing backslashes (even number of backslashes preceding the closing quotation mark)
                    cmdline.Append(new string('\\', 2 * ((argument.Length - 1) - p4)));
                    // closing quotation mark + space separator for next argument
                    cmdline.Append("\" ");
                }
            }
            if (cmdline.Length > 0)
            {
                //remove last space
                cmdline.Length--;
            }

            return cmdline.ToString();
        }

        public ConsoleExecutor(string executable, IList<string> commandlineParms, string workingdirectory, string enterPasswordText, SecureString password,
                               IList<string> AppendToPath = null, IDictionary<string, string> EnvironmentVariables = null)
        {
            Construct(executable, EscapeParmsToCommandline(commandlineParms), workingdirectory, enterPasswordText, password, AppendToPath, EnvironmentVariables);
        }

        public ConsoleExecutor(string executable, string commandline, string workingdirectory, string enterPasswordText, SecureString password,
                               IList<string> AppendToPath = null, IDictionary<string, string> EnvironmentVariables = null)
        {
            Construct(executable, commandline, workingdirectory, enterPasswordText, password, AppendToPath, EnvironmentVariables);
        }

        private void Construct(string executable, string commandline, string workingdirectory, string enterPasswordText, SecureString password,
                                   IList<string> AppendToPath = null, IDictionary<string, string> EnvironmentVariables = null)
        {
            _executable = executable;
            _commandline = (commandline != null ? commandline : String.Empty);
            _workingdirectory = workingdirectory;
            if (String.IsNullOrEmpty(_workingdirectory)) _workingdirectory = Path.GetDirectoryName(_executable);
            _enterPasswordText = enterPasswordText;
            _password = password;

            _appendToPath = AppendToPath;
            _environmentVariables = EnvironmentVariables;

            ExitCode = -1;

            Status = ConsoleStatus.ThreadStarting;
            _thread = new Thread(ThreadStart);
            _thread.Start(this);
        }

        public void Abort()
        {
            _threadabort = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ConsoleExecutor()
        {
            Dispose(false);
        }

        protected bool _isDisposed = false;
        protected void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (_process != null)
            {
                lock (_process)
                {
                    try { if (!_process.HasExited) _process.Kill(); } catch { }
                    _process.Dispose();
                }
            }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            public const int SW_HIDE = 0;
            public const int SW_SHOWNORMAL = 1;
            public const int SW_MINIMIZE = 6;

            public enum StdHandle : int
            {
                STD_INPUT_HANDLE = -10,
                STD_OUTPUT_HANDLE = -11,
                STD_ERROR_HANDLE = -12,
            }

            public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetStdHandle(int nStdHandle); //returns Handle

            public const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool AttachConsole(uint dwProcessId);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetConsoleWindow();

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern bool FreeConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
                out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);


            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadConsoleOutputCharacter(IntPtr hConsoleOutput,
               [Out] StringBuilder lpCharacter, uint nLength, COORD dwReadCoord,
               out uint lpNumberOfCharsRead);

            [StructLayout(LayoutKind.Sequential)]
            public struct CONSOLE_SCREEN_BUFFER_INFO
            {
                public COORD dwSize;
                public COORD dwCursorPosition;
                public ushort wAttributes;
                public SMALL_RECT srWindow;
                public COORD dwMaximumWindowSize;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct COORD
            {
                public short X;
                public short Y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SMALL_RECT
            {
                public short Left;
                public short Top;
                public short Right;
                public short Bottom;
            }

            [DllImportAttribute("user32.dll", EntryPoint = "BlockInput")]
            [return: MarshalAsAttribute(UnmanagedType.Bool)]
            public static extern bool BlockInput([MarshalAsAttribute(UnmanagedType.Bool)] bool fBlockIt);

            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);
        }

        private static void ThreadStart(object param)
        {
            ConsoleExecutor me = (ConsoleExecutor)param;

            try
            {
                me.Status = ConsoleStatus.ThreadStarted;
                try
                {
                    if (String.IsNullOrEmpty(me._enterPasswordText))
                    {
                        me.Start();
                        me.Status = ConsoleStatus.PasswordEntered;
                    }
                    else
                    {
                        me.StartWithPassword();
                    }

                    while (true)
                    {
                        while (true)
                        {
                            string text;

                            lock (me._inputQueue)
                            {
                                if (me._inputQueue.Count == 0)
                                {
                                    if (me._inputQueueClose == _closeState.Close)
                                    {
                                        me._inputQueueClose = _closeState.Closed;
                                        me._process.StandardInput.Flush();
                                        me._process.StandardInput.Close();
                                    }

                                    break;
                                }
                                text = me._inputQueue.Dequeue();
                            }

                            if (me._threadabort) break;
                            lock (me._process)
                            {
                                if (me._isDisposed) break;
                                if (me._process.WaitForExit(0))
                                {
                                    //This overload ensures that all processing has been completed, including the handling of asynchronous events for redirected standard output.
                                    //You should use this overload after a call to the WaitForExit(Int32) overload when standard output has been redirected to asynchronous event handlers.
                                    me._process.WaitForExit();
                                    break;
                                }
                                me._process.StandardInput.Write(text);
                            }
                        }

                        if (me._threadabort) break;
                        lock (me._process)
                        {
                            if (me._isDisposed) break;
                            if (me._process.WaitForExit(100))
                            {
                                //This overload ensures that all processing has been completed, including the handling of asynchronous events for redirected standard output.
                                //You should use this overload after a call to the WaitForExit(Int32) overload when standard output has been redirected to asynchronous event handlers.
                                me._process.WaitForExit();
                                break;
                            }
                        }
                    }

                    if (me._threadabort)
                    {
                        me.ExitCode = -2;
                        me.Result = ConsoleResult.Aborted;
                        return;
                    }

                    lock (me._process)
                    {
                        if (me._isDisposed)
                        {
                            me.ExitCode = -3;
                            me.Result = ConsoleResult.Disposed;
                            return;
                        }

                        me.ExitCode = me._process.ExitCode;
                        me.Result = ConsoleResult.Exited;
                    }
                }
                catch (Exception ex)
                {
                    me.ExitCode = -4;
                    me.ResultException = ex;
                    me.Result = ConsoleResult.Exception;
                }
            }
            finally
            {
                me._thread = null;
            }
        }

        private void Start()
        {
            Status = ConsoleStatus.ProcessStarting;

            _process = new Process();
            _process.StartInfo.FileName = _executable;
            _process.StartInfo.WorkingDirectory = _workingdirectory;
            _process.StartInfo.Arguments = _commandline;
            _process.StartInfo.UseShellExecute = false;

            if (_appendToPath != null)
            {
                string result = _process.StartInfo.EnvironmentVariables["Path"];
                foreach (var path in _appendToPath)
                {
                    result += ";" + path;
                }
                _process.StartInfo.EnvironmentVariables["Path"] = result;
            }

            if (_environmentVariables != null)
            {
                foreach (var item in _environmentVariables)
                {
                    _process.StartInfo.EnvironmentVariables[item.Key] = item.Value;
                }
            }

            if (String.IsNullOrEmpty(_enterPasswordText))
            {
                _process.StartInfo.CreateNoWindow = true; //Can't be used with password, see StartPassword()
            }

            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //Doesn't work for console windows

            _process.StartInfo.RedirectStandardInput = true;

            _process.StartInfo.RedirectStandardOutput = true;
            _process.OutputDataReceived += (sender, received) =>
            {
                lock (lockobj)
                {
                    if (!String.IsNullOrEmpty(received.Data))
                        _output.Append(received.Data);
                    _output.Append(Environment.NewLine);
                }
            };

            _process.StartInfo.RedirectStandardError = true;
            _process.ErrorDataReceived += (sender, received) =>
            {
                lock (lockobj)
                {
                    if (!String.IsNullOrEmpty(received.Data))
                        _error.Append(received.Data);
                    _error.Append(Environment.NewLine);
                }
            };

            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            Status = ConsoleStatus.ProcessStarted;
        }

        private IntPtr GetConsoleWindowHandle(Stopwatch watch)
        {
            IntPtr consoleHandle;

            while (true)
            {
                consoleHandle = NativeMethods.GetStdHandle((int)NativeMethods.StdHandle.STD_OUTPUT_HANDLE);
                int lasterror = Marshal.GetLastWin32Error();
                String error = String.Empty;

                if (consoleHandle == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    error = "STD_OUTPUT_HANDLE: (" + lasterror + ") " + (new Win32Exception(lasterror)).Message;
                }
                else if (consoleHandle == null)
                {
                    error = "STD_OUTPUT_HANDLE: got null handle";
                }
                else
                {
                    NativeMethods.CONSOLE_SCREEN_BUFFER_INFO csbi;
                    if (NativeMethods.GetConsoleScreenBufferInfo(consoleHandle, out csbi)) break;
                    lasterror = Marshal.GetLastWin32Error();

                    error = "STD_OUTPUT_HANDLE: GetConsoleScreenBufferInfo failed on handle (" + lasterror + ") " + (new Win32Exception(lasterror)).Message;
                }

                consoleHandle = NativeMethods.GetConsoleWindow();
                lasterror = Marshal.GetLastWin32Error();
                error += Environment.NewLine;

                if (consoleHandle == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    error += "GetConsoleWindow: (" + lasterror + ") " + (new Win32Exception(lasterror)).Message;
                }
                else if (consoleHandle == null)
                {
                    error += "GetConsoleWindow: got null handle";
                }
                else
                {
                    NativeMethods.CONSOLE_SCREEN_BUFFER_INFO csbi;
                    if (NativeMethods.GetConsoleScreenBufferInfo(consoleHandle, out csbi)) break;
                    error += "GetConsoleWindow: GetConsoleScreenBufferInfo failed on handle (" + lasterror + ") " + (new Win32Exception(lasterror)).Message;
                }

                if (watch.ElapsedMilliseconds > 30000)
                    throw new Exception("(When running under VS2019 IDE in debug mode this is known to fail. Start the program via the Windows Explorer.) Enter password: error getting console handle: 30 seconds: " + error);
                Thread.Sleep(10);
            }

            return consoleHandle;
        }

        private static object _startPasswordLock = new Object();
        private void StartWithPassword()
        {
            //THIS process can only have ONE console. Make sure this is only executed once at a time
            Status = ConsoleStatus.WaitForPasswordEnteringLock;
            lock (_startPasswordLock)
            {
                //Block keyboard and mouse, for accidental keypresses into new started executable. (that would disturb the password entering).
                NativeMethods.BlockInput(true);
                try
                {
                    Start();

                    Status = ConsoleStatus.PasswordEntering;
                    var watch = Stopwatch.StartNew();

                    //Wait for Enter password: to be shown on the console (not stdout)
                    try
                    {
                        //Attach to console of started executable
                        if (!NativeMethods.FreeConsole())
                        {
                            int lasterror = Marshal.GetLastWin32Error();
                            if (lasterror != 6 && /* ERROR_INVALID_HANDLE */
                                lasterror != 87 /* ERROR_INVALID_PARAMETER */)
                                throw new Exception("Enter password: error freeing console: (" + lasterror + ") " + (new Win32Exception(lasterror)).Message);
                        }

                        while (true)
                        {
                            _process.Refresh(); // discard cached information
                            if (_process.HasExited)
                            {
                                throw new Exception("Enter password: process is gone");
                            }

                            if (NativeMethods.AttachConsole((uint)_process.Id)) break;
                            int lasterror = Marshal.GetLastWin32Error();

                            switch (lasterror)
                            {
                                case 5: throw new Exception("Enter password: process is gone, error attaching console, already attached to a console (FreeConsole Failed): (" + lasterror + ") " + (new Win32Exception(lasterror)).Message);
                                case 87: throw new Exception("Enter password: process is gone, error attaching console, the specified process does not exist: (" + lasterror + ") " + (new Win32Exception(lasterror)).Message);

                                    //case 6: ERROR_INVALID_HANDLE --> console is not yet created
                                    //case 31: ERROR_GEN_FAILURE --> Windows Server 2008, console is not yet created
                            }

                            if (watch.ElapsedMilliseconds > 30000)
                                throw new Exception("Enter password: error attaching console: 30 seconds: (" + lasterror + ") " + (new Win32Exception(lasterror)).Message);
                            Thread.Sleep(10);
                        }

                        IntPtr consoleHandle = GetConsoleWindowHandle(watch);

                        //Read console line at cursor position until Enter password: is found
                        NativeMethods.CONSOLE_SCREEN_BUFFER_INFO csbi;
                        NativeMethods.COORD position;
                        StringBuilder linebuilder = new StringBuilder();
                        while (true)
                        {
                            _process.Refresh(); // discard cached information
                            if (_process.HasExited)
                            {
                                throw new Exception("Enter password: process is gone");
                            }

                            if (watch.ElapsedMilliseconds > 30000)
                                throw new Exception("Enter password: error reading console: 30 seconds");

                            if (!NativeMethods.GetConsoleScreenBufferInfo(consoleHandle, out csbi))
                            {
                                int lasterror = Marshal.GetLastWin32Error();
                                switch (lasterror)
                                {
                                    case 6: /* ERROR_INVALID_HANDLE */
                                        consoleHandle = GetConsoleWindowHandle(watch);
                                        continue;

                                    default:
                                        throw new Exception("Enter password: error getting console info: (" + lasterror + ") " + (new Win32Exception(lasterror)).Message);
                                }
                            }

                            position.X = 0;
                            position.Y = csbi.dwCursorPosition.Y;

                            linebuilder.Length = csbi.dwSize.X;
                            uint read = 0;
                            if (!NativeMethods.ReadConsoleOutputCharacter(consoleHandle, linebuilder, (uint)linebuilder.Length, position, out read))
                            {
                                int lasterror = Marshal.GetLastWin32Error();
                                switch (lasterror)
                                {
                                    case 6: /* ERROR_INVALID_HANDLE */
                                        consoleHandle = GetConsoleWindowHandle(watch);
                                        continue;

                                    default:
                                        throw new Exception("Enter password: error reading console: (" + lasterror + ") " + (new Win32Exception(lasterror)).Message);
                                }
                            }
                            linebuilder.Length = (int)read;

                            string line = linebuilder.ToString();
                            if (line.ToLowerInvariant().Contains(_enterPasswordText.ToLowerInvariant())) break;

                            Thread.Sleep(500);
                        }

                        //Enter password: is now shown, send password (not via stdin)

                        /*
                        (BlockInput) When input is blocked, real physical input from the mouse or keyboard will not affect the input queue's synchronous key state
                        (reported by GetKeyState and GetKeyboardState), nor will it affect the asynchronous key state (reported by GetAsyncKeyState).

                        However, the thread that is blocking input can affect both of these key states by calling SendInput. No other thread can do this.
                        */
                        _process.Refresh(); // discard cached information
                        NativeMethods.ShowWindow(_process.MainWindowHandle, NativeMethods.SW_SHOWNORMAL);
                        NativeMethods.SetForegroundWindow(_process.MainWindowHandle);

                        ConsoleSendInput.TypeInText(_password, true, 10);
                    }
                    finally
                    {
                        try { NativeMethods.FreeConsole(); } catch { }
                    }
                }
                finally
                {
                    NativeMethods.BlockInput(false);
                }
            }

            //Done, just in case another hide
            _process.Refresh(); // discard cached information
            if (_process.HasExited)
            {
                throw new Exception("Enter password: process is gone");
            }
            NativeMethods.ShowWindow(_process.MainWindowHandle, NativeMethods.SW_HIDE);

            Status = ConsoleStatus.PasswordEntered;
        }

        public void WaitForPasswordEntered()
        {
            ConsoleStatus status;
            ConsoleResult result;
            while (true)
            {
                status = Status;
                if (status == ConsoleStatus.PasswordEntered) break;

                result = Result;
                if (result != ConsoleResult.Busy)
                    throw new Exception("Process has stopped.");

                Thread.Sleep(100);
            }
        }

        public string WaitFor()
        {
            ConsoleResult result;
            while (true)
            {
                result = Result;
                if (result != ConsoleResult.Busy) break;

                Thread.Sleep(500);
            }


            bool success = false;
            string message;
            switch (result)
            {
                case ConsoleExecutor.ConsoleResult.Exited:
                    try
                    {
                        message = "[ ok ]";
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        message = "[ error ] " + ex.Message + (!String.IsNullOrEmpty(Error) ? " - " + Error : "");
                    }
                    break;
                case ConsoleExecutor.ConsoleResult.Exception:
                    message = "[ exception ] " + ResultException.Message + (!String.IsNullOrEmpty(Error) ? " - " + Error : "");
                    break;
                case ConsoleExecutor.ConsoleResult.Aborted:
                    message = "[ aborted ]";
                    break;
                case ConsoleExecutor.ConsoleResult.Disposed:
                    message = "[ disposed ]";
                    break;
                default:
                    message = "[unknown - " + result.ToString() + "]";
                    break;
            }

            if (!success)
            {
                throw new Exception(message);
            }

            return message;
        }
    }
}
