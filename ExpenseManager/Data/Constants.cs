namespace ExpenseManager.Data;

public static class Constants {
    public const int ExpensePerScreen = 8;
    public const ConsoleKey EndKey = ConsoleKey.Escape;
    public const string DateTimeFormat = "yyyy.MM.dd HH:mm";

    public const string Replacable = "#1#";

    public const string ErrorHeader = "[ error ]";

    public const string StartupHeader = "Welcome to CLI-Expense Manager";
    public static readonly string StartupInfo =
@$"Register [R]
Login [L]
Quit [{EndKey.ToString()}]";

    public static readonly string RegisterHeader = $"Register | Confirm [Enter] | Quit [{EndKey.ToString()}]";
    public static readonly string LoginHeader = $"Login | Confirm [Enter] | Quit [{EndKey.ToString()}]";

    public const string Header = $"Logged as: {Replacable} | [A] Add expense | [I] Add income | [C] New category | [D] Display balance | [F] Filter";

    public const string ExpenseHeader = @"|  ID  |   AMOUNT   |   CATEGORY   |       TIME       | DESCRIPTION";

    public static readonly string AddExpenseHeader = $"Adding Expense | [Enter] Confirm | [{EndKey.ToString()}] Quit ";
    public static readonly string AddIncomeHeader = $"Adding Income | [Enter] Confirm | [{EndKey.ToString()}] Quit ";

    private const string AddExpenseAmount = "Amount: ";
    private const string AddExpenseCategory = "Category: ";
    private const string AddExpenseDescription = "Description: ";
    private const string AddExpenseDate = "DateTime: ";
    public static readonly List<string> AddExpenseLines = [AddExpenseAmount, AddExpenseCategory, AddExpenseDescription, AddExpenseDate];

    public const string DisplayAmount = $"Current balance is {Replacable}";

    public static readonly string AddCategoryHeader = $"Adding Category | [Enter] Confirm | [{EndKey.ToString()}] Quit ";

    public static readonly string FilterHeader = $"Setting filters | [Enter] Confirm | [{EndKey.ToString()}] Quit ";
}

public static class Utils {
    public static string MakeErrorMessage(string input) {
        return string.Concat(Constants.ErrorHeader, " ", input);
    }
}

