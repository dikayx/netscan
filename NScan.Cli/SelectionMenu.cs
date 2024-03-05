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
        var consoleColor = GetRandomConsoleColor;
        ForegroundColor = consoleColor;
        WriteLine(_banner);
        ResetColor();
        WriteLine("\nUse ↑ and ↓ to navigate and press Enter/Return to select a scan method\n");
        (int left, int top) = GetCursorPosition();
        var option = 0;
        //var decorator = "\u001b[32m\u001b[1m>> "; // green
        var decorator = ConsoleColorToAnsi(consoleColor) + "\u001b[1m>> ";
        ConsoleKeyInfo key;
        bool isSelected = false;

        while (!isSelected)
        {
            SetCursorPosition(left, top);

            for (int i = 0; i < _options.Length; i++)
            {
                WriteLine($"{(option == i ? decorator : "   ")}{_options[i]}\u001b[0m");
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

    // Little gimmick: Get a random color for the banner
    private static ConsoleColor GetRandomConsoleColor => (ConsoleColor)new Random().Next(1, 8);

    static string ConsoleColorToAnsi(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.DarkBlue => "\u001b[34m",
            ConsoleColor.DarkGreen => "\u001b[32m",
            ConsoleColor.DarkCyan => "\u001b[36m",
            ConsoleColor.DarkRed => "\u001b[31m",
            ConsoleColor.DarkMagenta => "\u001b[35m",
            ConsoleColor.DarkYellow => "\u001b[33m",
            _ => "\u001b[32m",// default to green
        };
    }
}