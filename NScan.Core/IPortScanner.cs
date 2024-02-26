using System.Net;

namespace NScan.Core;

public interface IPortScanner
{
    // Single-threaded scan method
    List<int> Scan(IPAddress ipAddress, int startPort, int endPort, int timeoutMilliseconds, ref int openPorts, List<int> openPortList);

    // Multi-threaded scan method (async)
    Task<List<int>> ScanAsync(IPAddress ipAddress, int timeoutMilliseconds, int port, int openPorts, List<int> openPortList);
}
