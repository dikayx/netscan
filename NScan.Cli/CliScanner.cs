using NScan.Core;
using System.Net;

namespace NScan.Cli;

public class CliScanner(string target, int startPort, int endPort, int timeoutMilliseconds)
{
    private readonly string _target = target;
    private readonly int _startPort = startPort;
    private readonly int _endPort = endPort;
    private readonly int _timeoutMilliseconds = timeoutMilliseconds;

    public async Task<List<int>> PerformScan(ScanMethod scanMethod)
    {
        // If the target is an IP address, this will return the IP address;
        // otherwise, it will perform a DNS lookup

        #pragma warning disable CS8600 // address is checked by implementation
        if (!IPAddress.TryParse(_target, out IPAddress ipAddress))
        {
            var host = await Dns.GetHostEntryAsync(_target);
            ipAddress = host.AddressList[0];
        }

        ScanService scanService = new(ipAddress, _startPort, _endPort, _timeoutMilliseconds);
        ProgressService progressService = new(scanService, _startPort, _endPort);

        // Scanning and progress rendering are independent tasks
        Task progressTask = RenderProgress(progressService);
        Task<List<int>> scanTask = scanService.ScanPorts(scanMethod);
        await Task.WhenAll(progressTask, scanTask);

        return await scanTask;
    }

    public static async Task RenderProgress(ProgressService progressService)
    {
        using var progressBar = new ProgressBar();
        while (true)
        {
            float progress = progressService.GetProgress();
            progressBar.Report(progress);

            // Check if scanning is complete
            if (progress >= 1.0f) break;

            await Task.Delay(100);
        }
    }
}
