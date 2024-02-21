using System.Net;
using System.Net.Sockets;

// Program
string developer = "Dan Koller";
string version = "0.1.0";
string note = $"Version {version} by {developer}";
string banner = @"
-----------------------------------------
  _   _      _    _____                 
 | \ | |    | |  / ____|                
 |  \| | ___| |_| (___   ___ __ _ _ __  
 | . ` |/ _ \ __|\___ \ / __/ _` | '_ \ 
 | |\  |  __/ |_ ____) | (_| (_| | | | |
 |_| \_|\___|\__|_____/ \___\__,_|_| |_|

 " + note + @"
-----------------------------------------";

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
IPAddress[] ipAddresses = Dns.GetHostAddresses(target);

// Reduce timeout to speed up closed port detection
int timeoutMilliseconds = 100;

// Record start time
DateTime startTime = DateTime.Now;

WriteLine($"Scanning ports {startPort} to {endPort} on {target}...");
for (int port = startPort; port <= endPort; port++)
{
    using (var client = new TcpClient())
    {
        IAsyncResult result = client.BeginConnect(ipAddress, port, null, null);
        WaitHandle timeoutHandler = result.AsyncWaitHandle;

        try
        {
            if (!result.AsyncWaitHandle.WaitOne(timeoutMilliseconds, false))
            {
                client.Close();
                WriteLine($"Port {port} is closed");
                continue;
            }

            client.EndConnect(result);
            WriteLine($"Port {port} is open");
            client.Close();
        }
        catch
        {
            WriteLine($"Port {port} is closed");
        }
        finally
        {
            timeoutHandler.Close();
        }
    }
}

// Record end time
DateTime endTime = DateTime.Now;
TimeSpan timeTaken = endTime - startTime;
WriteLine($"Scan completed in {timeTaken.TotalSeconds} seconds");
