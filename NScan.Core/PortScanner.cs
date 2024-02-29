using System.Net;
using System.Net.Sockets;

namespace NScan.Core
{
    public class PortScanner : IPortScanner
    {
        private int _portsScanned;

        public PortScanner()
        {
            _portsScanned = 0;
        }

        public int PortsScanned => _portsScanned;

        // Single-threaded scan method
        public List<int> Scan(IPAddress ipAddress, int timeoutMilliseconds, int startPort, int endPort, ref int openPorts, List<int> openPortList)
        {
            for (int port = startPort; port <= endPort; port++)
            {
                try
                {
                    using var client = new TcpClient();
                    IAsyncResult result = client.BeginConnect(ipAddress, port, null, null);
                    WaitHandle timeoutHandler = result.AsyncWaitHandle;

                    Interlocked.Increment(ref _portsScanned);

                    if (!timeoutHandler.WaitOne(timeoutMilliseconds, false))
                    {
                        //WriteLine($"Port {port} is closed");
                    }
                    else
                    {
                        client.EndConnect(result);
                        //PrintError($"Port {port} is open");
                        Interlocked.Increment(ref openPorts);
                        lock (openPortList)
                        {
                            openPortList.Add(port);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLine($"Error scanning port {port}: {ex.Message}");
                }
            }

            return openPortList;
        }

        // Multi-threaded scan method (async)
        public async Task<List<int>> ScanAsync(IPAddress ipAddress, int timeoutMilliseconds, int port, int openPorts, List<int> openPortList)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ipAddress, port);

                await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds));

                Interlocked.Increment(ref _portsScanned);

                if (connectTask.IsCompletedSuccessfully && client.Connected)
                {
                    //PrintError($"Port {port} is open");
                    openPorts++; // Increment open port count
                    lock (openPortList)
                    {
                        openPortList.Add(port); // Add the open port to the list
                    }
                }
                else
                {
                    //WriteLine($"Port {port} is closed");
                }
            }
            catch (Exception ex)
            {
                WriteLine($"Error scanning port {port}: {ex.Message}");
            }

            return openPortList;
        }

        // Helper method for printing open port messages
        private static void PrintError(string msg)
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine(msg);
            ResetColor();
        }
    }
}
