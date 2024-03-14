using System.Text;

namespace NScan.Cli;

public class SelectionMenu(string banner, string[] options)
{
    private readonly string _banner = banner;
    private readonly string[] _options = options;

    public int ShowMenu()
    {
        Clear();
        OutputEncoding = Encoding.UTF8;
        CursorVisible = false;
        ForegroundColor = ConsoleColor.White;
        WriteLine(_banner);
        ResetColor();
        WriteLine("\nUse ↑ and ↓ to navigate and press Enter/Return to select a scan method\n");
        (int left, int top) = GetCursorPosition();
        var option = 0;
        var decorator = ConsoleSupportsAnsi() ? "\u001b[32m\u001b[1m>> " : ">> "; // green or default
        ConsoleKeyInfo key;
        bool isSelected = false;

        while (!isSelected)
        {
            SetCursorPosition(left, top);

            for (int i = 0; i < _options.Length; i++)
            {
                // If the console supports ANSI escape codes, we can use them to highlight the selected option
                // Otherwise, we'll just use the default console colors
                string output = $"{(option == i ? decorator : "   ")}{_options[i]}";
                if (ConsoleSupportsAnsi()) output += "\u001b[0m";
                WriteLine(output);
            }

            key = ReadKey(false);

            int optionsCount = _options.Length - 1;
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    option = option == 0 ? optionsCount : option - 1;
                    break;

                case ConsoleKey.DownArrow:
                    option = option == optionsCount ? 0 : option + 1;
                    break;

                case ConsoleKey.Enter:
                    isSelected = true;
                    break;
            }
        }

        CursorVisible = true;
        return option;
    }

    // ANSI escape codes are only supported on Unix and macOS
    private static bool ConsoleSupportsAnsi()
    {
        return Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
    }
}