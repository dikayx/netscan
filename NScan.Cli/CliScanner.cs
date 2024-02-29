using NScan.Core;
using System.Net;

namespace NScan.Cli;

public class CliScanner
{
    private string _target;
    private int _startPort;
    private int _endPort;
    private int _timeoutMilliseconds;
    private float _progress;

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
        // Start the rendering of progress bars asynchronously
        Task progressTask = RenderProgress(progressService);

        // Start scanning ports asynchronously
        Task<List<int>> scanTask = scanService.ScanPorts(scanMethod);

        // Wait for both tasks to complete
        await Task.WhenAll(progressTask, scanTask);

        return await scanTask;
    }

    public async Task RenderProgress(ProgressService progressService)
    {
        // Update the progress continuously until scanning is complete
        while (true)
        {
            // Get the current progress
            float progress = progressService.GetProgress();

            // Render the progress bar
            RenderProgressBar(progress);

            // Check if scanning is complete
            if (progress >= 1.0f)
                break;

            // Wait for a short interval before updating again
            await Task.Delay(100);
        }
    }
    public void RenderProgressBar(float progress)
    {
        // Calculate the width of the progress bar
        int barWidth = Console.WindowWidth - 10;
        int progressWidth = (int)(barWidth * progress);

        // Render the progress bar
        Console.Write("[");
        Console.Write(new string('=', progressWidth));
        Console.Write(new string(' ', barWidth - progressWidth));
        Console.Write($"] {progress:P}");
        Console.CursorLeft = 0; // Move the cursor to the beginning of the line
    }
}
