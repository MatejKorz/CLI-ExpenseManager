namespace ExpenseManager.IO;

public static class Highlighter {
    public static void Write(string content, ConsoleColor? foreground = null,
        ConsoleColor? background = null) {
        var originalForeground = Console.ForegroundColor;
        var originalBackground = Console.BackgroundColor;
        if (foreground.HasValue) {
            Console.ForegroundColor = foreground.Value;
        }

        if (background.HasValue) {
            Console.BackgroundColor = background.Value;
        }
        Console.Write(content);
        Console.ForegroundColor = originalForeground;
        Console.BackgroundColor = originalBackground;
    }

    public static void WriteLine(string content, ConsoleColor? foreground = null,
        ConsoleColor? background = null) {
        var originalForeground = Console.ForegroundColor;
        var originalBackground = Console.BackgroundColor;
        if (foreground.HasValue) {
            Console.ForegroundColor = foreground.Value;
        }

        if (background.HasValue) {
            Console.BackgroundColor = background.Value;
        }
        Console.WriteLine(content);
        Console.ForegroundColor = originalForeground;
        Console.BackgroundColor = originalBackground;
    }
}