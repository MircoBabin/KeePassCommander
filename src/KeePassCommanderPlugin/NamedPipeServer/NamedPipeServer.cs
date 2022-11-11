using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;

namespace KeePassCommander.NamedPipeServer
{
    public class NamedPipeServer
    {
        private readonly DebugLog Debug;
        private readonly Command.Runner Runner;

        private string ServerPipeName;

        public NamedPipeServer(DebugLog Debug, Command.Runner Runner)
        {
            this.Debug = Debug;
            this.Runner = Runner;

            ServerPipeName = "KeePassCommander." + Process.GetCurrentProcess().SessionId;
        }

        public void StartServer()
        {
            Thread ServerThread = new Thread(ThreadStartServer);
            ServerThread.Start(this);
        }

        public void StopServer()
        {
            Stop = true;

            lock (ServerLock)
            {
                try
                {
                    using (NamedPipeClientStream npcs = new NamedPipeClientStream(".", ServerPipeName, PipeDirection.InOut, PipeOptions.None))
                    {
                        npcs.Connect(100);
                    }
                }
                catch { }

                foreach (NamedPipeServerConnection connection in ServerConnections)
                {
                    try
                    {
                        connection.Close();
                    }
                    catch { }
                }
            }
        }

        private object ServerLock = new object();
        private Boolean Stop = false;
        private NamedPipeServerStream ServerPipe = null;
        private List<NamedPipeServerConnection> ServerConnections = new List<NamedPipeServerConnection>();

        private static void ThreadStartServer(object data)
        {
            NamedPipeServer me = data as NamedPipeServer;
            if (me != null)
                try
                {
                    me.RunServer();
                }
                catch { }
        }

        private void RunServer()
        {
            if (Stop)
            {
                Debug.OutputLine("Not starting Named Pipe Server, Stop=true (1)");
                return;
            }

            lock (ServerLock)
            {
                if (Stop)
                {
                    Debug.OutputLine("Not starting Named Pipe Server, Stop=true (2)");
                    return;
                }

                try
                {
                    Debug.OutputLine("Starting Named Pipe Server on \"" + ServerPipeName + "\"");
                    ServerPipe = new NamedPipeServerStream(ServerPipeName,
                        PipeDirection.InOut,
                        10,
                        PipeTransmissionMode.Byte,
                        PipeOptions.None);
                    Debug.OutputLine("Named Pipe Server started");
                }
                catch (Exception ex)
                {
                    Debug.OutputLine("Starting Named Pipe Server failed" + Environment.NewLine + ex.ToString());
                    Stop = true;
                    return;
                }
            }

            try
            {
                Debug.OutputLine("Waiting for connection");
                ServerPipe.WaitForConnection();
                Debug.OutputLine("Connection received");

                if (Stop)
                {
                    Debug.OutputLine("Ending connection, Stop=true (3)");
                    ServerPipe.Dispose();
                    ServerPipe = null;
                    return;
                }

                NamedPipeServerConnection connection;
                lock (ServerLock)
                {
                    if (Stop)
                    {
                        Debug.OutputLine("Ending connection, Stop=true (4)");
                        ServerPipe.Dispose();
                        ServerPipe = null;
                        return;
                    }

                    connection = new NamedPipeServerConnection(Debug, ServerPipe, ConnectionFinished);
                    ServerConnections.Add(connection);
                }

                Debug.OutputLine("Restart listening on named pipe");
                ServerPipe = null;
                StartServer();

                Debug.OutputLine("Starting run client");
                connection.Run(Runner);
                Debug.OutputLine("Ended run client");
            }
            catch (Exception ex)
            {
                Debug.OutputLine("RunServer Exception:" + Environment.NewLine + ex.ToString());
            }
        }

        private void ConnectionFinished(NamedPipeServerConnection connection)
        {
            if (Stop) return;
            lock (ServerLock)
            {
                if (Stop) return;

                try
                {
                    ServerConnections.Remove(connection);
                }
                catch { }
            }
        }

}
}
