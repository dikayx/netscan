namespace NScan.Core;

public class ProgressService(ScanService scanService, int startPort, int endPort)
{
    private readonly ScanService _scanService = scanService;
    private readonly int _startPort = startPort;
    private readonly int _endPort = endPort;

    public float GetProgress()
    {
        int portsScanned = _scanService.GetPortsScanned();
        int totalPorts = _endPort - _startPort + 1;
        return (float)portsScanned / totalPorts;
    }
}
