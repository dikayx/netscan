using NScan.Cli;
using NScan.Core;
using System.Net;
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

// WriteLine(banner);
// WriteLine("Please select a scan method:");

string[] options = [
    "Multi-threaded scan (fast, recommended)",
    "Single-threaded scan (slow, needs less resources)",
    "Custom scan (not yet implemented)",
    "Exit"
    ];
SelectionMenu2 menu = new(banner, options);
int selectedIndex = menu.ShowMenu();

// Options
string target = "www.google.com";
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
        startPort = GetPortFromUser();
        endPort = GetPortFromUser();
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
    WriteLine("Open ports:");
    PrintOpenPorts(openPortList);
}

// TODO: If the user enters nothing, use a default value

// Helper methods
static string GetTargetFromUser()
{
    Write("Enter the target IP address or domain name: ");
    // Target can be a number or domain name
    // domain must start with http:// or https:// or www.
    string regex = @"^(http|https|www)\:\/\/";

    // TODO: Regex checks are not working
    string target = ReadLine()!;
    if (Regex.IsMatch(target, regex))
    {
        WriteLine("Invalid target");
        GetTargetFromUser();
    }

    return target;
}

static int GetPortFromUser()
{
    // Must be a number between 1 and 65535
    Write("Enter the port number: ");
    string input = ReadLine()!;
    if (!int.TryParse(input, out int port) || port < 1 || port > 65535)
    {
        WriteLine("Invalid port number");
        GetPortFromUser();
    }
    return port;
}

static int GetTimeoutFromUser()
{
    // Must be a number between 1 and 10000
    Write("Enter the timeout in milliseconds: ");
    string input = ReadLine()!;
    if (!int.TryParse(input, out int timeout) || timeout < 1 || timeout > 10000)
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
