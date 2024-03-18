namespace NScan.Core
{
    public static class FileLogger
    {
        private static readonly string _logFilePath;

        static FileLogger()
        {
            // Get the application's root directory
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _logFilePath = Path.Combine(rootDirectory, "log.txt");
        }

        public static void Log(string message)
        {
            // Append the message to the log file
            File.AppendAllText(_logFilePath, $"{DateTime.Now} - {message}{Environment.NewLine}");
        }
    }
}
