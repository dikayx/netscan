using NScan.Cli;
using NScan.Core;
using System.Text.RegularExpressions;

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

string[] options = [
    "Multi-threaded scan (fast, recommended)",
    "Single-threaded scan (slow, needs less resources)",
    "Custom scan (enter target, start and end ports, and timeout)",
    "Exit"
    ];
SelectionMenu menu = new(banner, options);
int selectedIndex = menu.ShowMenu();

// Options
string target = "localhost";
int startPort = 1;
int endPort = 1024;
int timeoutMilliseconds = 100;

ScanMethod scanMethod = ScanMethod.MultiThreaded; // default
switch (selectedIndex)
{
    case 0:
        scanMethod = ScanMethod.MultiThreaded;
        break;
    case 1:
        scanMethod = ScanMethod.SingleThreaded;
        break;
    case 2:
        target = GetTargetFromUser();
        startPort = GetPortFromUser("start");
        endPort = GetPortFromUser("end");
        timeoutMilliseconds = GetTimeoutFromUser();
        break;
    case 3:
        Terminate("Exiting...");
        break;
    default:
        Terminate("Invalid selection"); // should never happen
        break;
}

// Record start time
DateTime startTime = DateTime.Now;

WriteLine($"Scanning ports {startPort} to {endPort} on {target}...");

// Create a new ScanService
CliScanner scanner = new(target, startPort, endPort, timeoutMilliseconds);
Task<List<int>> openPortsTask = scanner.PerformScan(scanMethod);
List<int> openPortList = openPortsTask.Result;
int openPorts = openPortList.Count;

// Record end time
DateTime endTime = DateTime.Now;
TimeSpan timeTaken = endTime - startTime;

// Print results
WriteLine($"Scan completed in {timeTaken.Minutes}:{timeTaken.Seconds} minutes");
WriteLine($"Found {openPorts} open ports");
if (openPorts > 0)
{
    PrintOpenPorts(openPortList);
}

// Helper methods
static string GetTargetFromUser()
{
    Write("Enter the target IP address or domain name (default: 127.0.0.1): ");
    // Target can be an IP address or a domain name
    string regex = @"^(?:https?:\/\/|www\.)|(?:\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";

    string? inputTarget = ReadLine()?.Trim();
    string target = string.IsNullOrWhiteSpace(inputTarget) ? "127.0.0.1" : inputTarget;

    if (!Regex.IsMatch(target, regex))
    {
        ForegroundColor = ConsoleColor.Red;
        WriteLine("Invalid input. Target must start with http://, https://, www., or be an IP address");
        ResetColor();
        target = GetTargetFromUser();
    }

    return target;
}

static int GetPortFromUser(string startOrEnd)
{
    // When startOrEnd is "start", the default port is 1; when it's "end", the default port is 65535
    Write($"Enter the {startOrEnd} port number (default: {(startOrEnd == "start" ? 1 : 65535)}): ");
    string? inputPort = ReadLine()?.Trim();
    string input = string.IsNullOrWhiteSpace(inputPort) ? (startOrEnd == "start" ? "1" : "65535") : inputPort;

    if (!int.TryParse(input, out int port) || port < 1 || port > 65535)
    {
        WriteLine("Invalid port number");
        GetPortFromUser(startOrEnd);
    }
    return port;
}

static int GetTimeoutFromUser()
{
    // Must be a number between 1 and 1000
    Write("Enter the timeout in milliseconds (default: 100): ");
    string? inputTimeout = ReadLine()?.Trim();
    string input = string.IsNullOrWhiteSpace(inputTimeout) ? "100" : inputTimeout;

    if (!int.TryParse(input, out int timeout) || timeout < 1 || timeout > 1000)
    {
        WriteLine("Invalid timeout");
        GetTimeoutFromUser();
    }
    return timeout;
}

static void PrintOpenPorts(List<int> openPortList)
{
    foreach (var port in openPortList)
    {
        ForegroundColor = ConsoleColor.Red;
        WriteLine(port);
        ResetColor();
    }
}

static void Terminate(string message)
{
    WriteLine(message);
    Environment.Exit(1);
}
