namespace ExpenseManager.Data;

public static class Constants {
    public const int ExpensePerScreen = 6;
    public const ConsoleKey EndKey = ConsoleKey.Escape;
    public const string DateTimeFormat = "yyyy.MM.dd HH:mm";

    public const string Replacable = "#1#";
    public const string Replacable2 = "#2#";

    public static readonly string Startup = @$"
Register [R]   |   Login [L]   |   Quit [{EndKey.ToString()}]
";

    public const string Username = @"
Username: ";

    public const string Password = @"Password: ";

    public const string Header = $"Logged as: {Replacable} | [D] Display balance | [A] Add expense | [F] Filter";

    public const string ExpenseHeader = @"|  ID  |   AMOUNT   |   CATEGORY   |       TIME       | DESCRIPTION";

    public static readonly string AddExpenseHeader = $"Adding Expense | [Enter] Confirm | [{EndKey.ToString()}] Quit ";
    private const string AddExpenseAmount = "Amount: ";
    private const string AddExpenseCategory = "Category: ";
    private const string AddExpenseDescription = "Description: ";
    public static readonly List<string> AddExpenseLines = [AddExpenseAmount, AddExpenseCategory, AddExpenseDescription];

    public static readonly string FilterHeader = $"Setting filters | [Enter] Confirm | [{EndKey.ToString()}] Quit ";

}

