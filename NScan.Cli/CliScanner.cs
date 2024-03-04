using NScan.Core;
using System.Net;
using NScan.Cli.Rendering;

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
        ProgressService progressService = new ProgressService(scanService, _startPort, _endPort);

        // Scanning and progress rendering are independent tasks
        Task progressTask = RenderProgress(progressService);
        Task<List<int>> scanTask = scanService.ScanPorts(scanMethod);
        await Task.WhenAll(progressTask, scanTask);

        return await scanTask;
    }

    public async Task RenderProgress(ProgressService progressService)
    {
        using var progressBar = new ProgressBar();
        while (true)
        {
            float progress = progressService.GetProgress();

            // Render the progress bar
            progressBar.Report(progress);

            // Check if scanning is complete
            if (progress >= 1.0f) break;

            await Task.Delay(100);
        }
    }
}
