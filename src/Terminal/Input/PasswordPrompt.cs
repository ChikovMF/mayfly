using System.Text;

namespace Mayfly.Terminal.Input;

internal static class PasswordPrompt
{
    public static bool IsAvailable => !Console.IsInputRedirected;

    public static string Read(string label)
    {
        Console.Error.Write($"{label}: ");

        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
            }
        }

        Console.Error.WriteLine();
        return password.ToString();
    }
}
