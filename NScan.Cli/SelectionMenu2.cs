using System.Text;

public class SelectionMenu2
{
    private string _banner;
    private string[] _options;

    public SelectionMenu2(string banner, string[] options)
    {
        _banner = banner;
        _options = options;
    }

    public int ShowMenu()
    {
        Clear();
        OutputEncoding = Encoding.UTF8;
        CursorVisible = false;
        ForegroundColor = ConsoleColor.Cyan;
        WriteLine(_banner);
        ResetColor();
        WriteLine("\nUse ⬆️  and ⬇️  to navigate and press \u001b[32mEnter/Return\u001b[0m to select a scan method\n");
        (int left, int top) = GetCursorPosition();
        var option = 0;
        var decorator = "\u001b[32m\u001b[1m>> ";
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
}