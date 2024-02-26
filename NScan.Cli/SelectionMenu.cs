namespace NScan.Cli;

public class SelectionMenu(string[] options)
{
    private readonly string[] _options = options;

    public int ShowMenu()
    {
        int selectedIndex = 0;
        int topPosition = CursorTop;

        ConsoleKeyInfo keyInfo;
        do
        {
            SetCursorPosition(0, topPosition);
            for (int i = 0; i < _options.Length; i++)
            {
                SetCursorPosition(0, topPosition + i);
                if (i == selectedIndex)
                {
                    ForegroundColor = ConsoleColor.Green;
                    Write(">> ");
                }
                else
                {
                    Write("   ");
                }
                WriteLine(_options[i]);
                ResetColor();
            }

            keyInfo = ReadKey(true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex - 1 + _options.Length) % _options.Length;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex + 1) % _options.Length;
            }

        } while (keyInfo.Key != ConsoleKey.Enter);

        WriteLine(); // Move to the next line after selecting an option
        return selectedIndex;
    }

}
