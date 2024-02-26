using System.Net;
using System.Net.Sockets;

// Program
string developer = "Dan Koller";
string version = "0.1.0";
string note = $"Version {version} by {developer}";
string banner = @"
------------------------------------------------------------
  _   _      _    _____                  _____ _      _____ 
 | \ | |    | |  / ____|                / ____| |    |_   _|
 |  \| | ___| |_| (___   ___ __ _ _ __ | |    | |      | |  
 | . ` |/ _ \ __|\___ \ / __/ _` | '_ \| |    | |      | |  
 | |\  |  __/ |_ ____) | (_| (_| | | | | |____| |____ _| |_ 
 |_| \_|\___|\__|_____/ \___\__,_|_| |_|\_____|______|_____|
 " + note + @"
------------------------------------------------------------";

WriteLine(banner);

WriteLine("Press any key to start the scan...");
ReadKey();

// Hardcoded for now
string target = "www.google.com";
var host = Dns.GetHostEntry(target);
IPAddress ipAddress = host.AddressList[0];

int startPort = 1;
int endPort = 1024;

// Pre-resolve IP address
//IPAddress[] ipAddresses = Dns.GetHostAddresses(target);

// Reduce timeout to speed up closed port detection
int timeoutMilliseconds = 100;

// Record start time
DateTime startTime = DateTime.Now;

WriteLine($"Scanning ports {startPort} to {endPort} on {target}...");

int openPorts = 0;
List<int> openPortList = [];

// Multithreading setup
int threadCount = GetThreadCount();
Thread[] threads = new Thread[threadCount];
// Scan settings
bool inOrderScan = false;
bool singleThreaded = false;

// Calculate the number of ports each thread should handle
int portsPerThread = (endPort - startPort + 1) / threadCount;

if (singleThreaded)
{
    ScanPortsSingleThread(ipAddress, startPort, endPort, timeoutMilliseconds, ref openPorts, openPortList);
}
else
{
    if (inOrderScan)
    {
        // Create a lock object
        object portLock = new object();
        int nextPort = startPort;

        for (int i = 0; i < threadCount; i++)
        {
            // Start a new thread
            threads[i] = new Thread(() => ScanPortsInOrder(ipAddress, timeoutMilliseconds, ref nextPort, endPort, portLock, ref openPorts, openPortList));
            threads[i].Start();
        }
    }
    else
    {
        for (int i = 0; i < threadCount; i++)
        {
            int threadStartPort = startPort + i * portsPerThread;
            int threadEndPort = i == threadCount - 1 ? endPort : threadStartPort + portsPerThread - 1;

            // Start a new thread
            threads[i] = new Thread(() => ScanPorts(ipAddress, threadStartPort, threadEndPort, timeoutMilliseconds, ref openPorts, openPortList));
            threads[i].Start();
        }
    }

    // Wait for all threads to complete
    foreach (var thread in threads)
    {
        thread.Join();
    }
}

DateTime endTime = DateTime.Now;
TimeSpan timeTaken = endTime - startTime;

WriteLine($"Scan completed in {timeTaken.Minutes}:{timeTaken.Seconds} minutes");
WriteLine($"Found {openPorts} open ports");
if (openPorts > 0)
{
    WriteLine("Open ports:");
    PrintOpenPorts(openPortList);
}

// Multithreaded port scanning without locks (out of order, fastest)
static void ScanPorts(IPAddress ipAddress, int startPort, int endPort, int timeoutMilliseconds, ref int openPorts, List<int> openPortList)
{
    for (int port = startPort; port <= endPort; port++)
    {
        try
        {
            using var client = new TcpClient();
            IAsyncResult result = client.BeginConnect(ipAddress, port, null, null);
            WaitHandle timeoutHandler = result.AsyncWaitHandle;

            if (!timeoutHandler.WaitOne(timeoutMilliseconds, false))
            {
                WriteLine($"Port {port} is closed");
            }
            else
            {
                client.EndConnect(result);
                PrintError($"Port {port} is open");
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
}

// Multithreaded port scanning with locks (in thread order, still fast, more readable)
static void ScanPortsInOrder(IPAddress ipAddress, int timeoutMilliseconds, ref int nextPort, int endPort, object portLock, ref int openPorts, List<int> openPortList)
{
    while (true)
    {
        // Lock access to the port variable
        int port;
        lock (portLock)
        {
            // Retrieve the next port to scan and increment the counter
            port = nextPort++;
        }

        // Check if we have scanned all ports
        if (port > endPort)
            break;

        try
        {
            using var client = new TcpClient();
            IAsyncResult result = client.BeginConnect(ipAddress, port, null, null);
            WaitHandle timeoutHandler = result.AsyncWaitHandle;

            if (!timeoutHandler.WaitOne(timeoutMilliseconds, false))
            {
                WriteLine($"Port {port} is closed");
            }
            else
            {
                client.EndConnect(result);
                PrintError($"Port {port} is open");
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
}

// Linear, single-threaded port scanning (slow, but reliable and great for logging)
static void ScanPortsSingleThread(IPAddress ipAddress, int startPort, int endPort, int timeoutMilliseconds, ref int openPorts, List<int> openPortList)
{
    for (int port = startPort; port <= endPort; port++)
    {
        try
        {
            using var client = new TcpClient();
            IAsyncResult result = client.BeginConnect(ipAddress, port, null, null);
            WaitHandle timeoutHandler = result.AsyncWaitHandle;

            if (!timeoutHandler.WaitOne(timeoutMilliseconds, false))
            {
                WriteLine($"Port {port} is closed");
            }
            else
            {
                client.EndConnect(result);
                PrintError($"Port {port} is open");
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
}

// Helper method to return a viable number of threads for the current system
static int GetThreadCount()
{
    return GetThreadCountWithMultiplier(1);
}

static int GetThreadCountWithMultiplier(int multiplier)
{
    int threadCount = Environment.ProcessorCount * multiplier;

    // Ensure thread count is within bounds
    if (threadCount < 1)
    {
        threadCount = 1;
    }
    else if (threadCount > 64)
    {
        threadCount = 64;
    }

    WriteLine($"Using {threadCount} threads");

    return threadCount;
}


// Format output
static void PrintError(string msg)
{
    ForegroundColor = ConsoleColor.Red;
    WriteLine(msg);
    ResetColor();
}

static void PrintOpenPorts(List<int> openPortList)
{
    foreach (var port in openPortList)
    {
        WriteLine(port);
    }
}
