using NScan.Core;
using System.Net;

namespace NScan.Cli;

public class CliScanner
{
    private string _target;
    private int _startPort;
    private int _endPort;
    private int _timeoutMilliseconds;

    public CliScanner(string target, int startPort, int endPort, int timeoutMilliseconds)
    {
        _target = target;
        _startPort = startPort;
        _endPort = endPort;
        _timeoutMilliseconds = timeoutMilliseconds;
    }

    public async Task<List<int>> PerformScan(ScanMethod scanMethod)
    {
        var host = Dns.GetHostEntry(_target);
        IPAddress ipAddress = host.AddressList[0];

        ScanService scanService = new ScanService(ipAddress, _startPort, _endPort, _timeoutMilliseconds);
        return await scanService.ScanPorts(scanMethod);
    }
}
