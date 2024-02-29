namespace NScan.Core;

public class ProgressService
{
    private ScanService _scanService;
    private int _startPort;
    private int _endPort;

    public ProgressService(ScanService scanService, int startPort, int endPort)
    {
        _scanService = scanService;
        _startPort = startPort;
        _endPort = endPort;
    }

    public float GetProgress()
    {
        int portsScanned = _scanService.GetPortsScanned();
        int totalPorts = _endPort - _startPort + 1;
        return (float)portsScanned / totalPorts;
    }
}
