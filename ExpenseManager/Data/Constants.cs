namespace ExpenseManager.Data;

public static class Constants {
    public const int CategoriesPerScreen = 10;
    public const ConsoleKey ConfirmKey = ConsoleKey.Enter;
    public const ConsoleKey EndKey = ConsoleKey.Escape;
    public const string DateTimeFormat = "yyyy.MM.dd HH:mm";
    public const string AllowedChars = "-_";

    public const string Replacable = "#1#";

    public const string ErrorHeader = "[ error ]";

    public const string StartupHeader = "Welcome to CLI-Expense Manager";
    public static readonly string StartupInfo =
@$"Register [R]
Login [L]
Quit [{EndKey.ToString()}]";

    public static readonly string RegisterHeader = $"Register | Confirm [{ConfirmKey.ToString()}] | Quit [{EndKey.ToString()}]";
    public static readonly string LoginHeader = $"Login | Confirm [{ConfirmKey.ToString()}] | Quit [{EndKey.ToString()}]";

    public const string Header = $"Logged as: {Replacable} | [A] Add expense | [I] Add income | [C] New category | [D] Display balance | [F] Filter | [J] JSON-Export/Import | [S] Statistics";

    public const string ExpenseHeader = @"|  ID  |   AMOUNT   |   CATEGORY   |       TIME       | DESCRIPTION";

    public static readonly string AddExpenseHeader = $"Adding Expense | [{ConfirmKey.ToString()}] Confirm | [{EndKey.ToString()}] Quit ";
    public static readonly string AddIncomeHeader = $"Adding Income | [{ConfirmKey.ToString()}] Confirm | [{EndKey.ToString()}] Quit ";

    private const string AddExpenseAmount = "Amount: ";
    private const string AddExpenseCategory = "Category: ";
    private const string AddExpenseDescription = "Description: ";
    private const string AddExpenseDate = "DateTime: ";
    public static readonly List<string> AddExpenseLines = [AddExpenseAmount, AddExpenseCategory, AddExpenseDescription, AddExpenseDate];

    public const string DisplayAmount = $"[ balance ] Current balance is {Replacable}";

    public static readonly string AddCategoryHeader = $"Adding Category | [{ConfirmKey.ToString()}] Confirm | [{EndKey.ToString()}] Quit ";

    public static readonly string FilterHeader = $"Setting filters | [P] Pick category | [R] Reset | [{ConfirmKey.ToString()}] Confirm | [{EndKey.ToString()}] Quit ";
    public const string FilterCategorySelected = "\u25a0";
    public const string FilterCategoryNotSelected = "\u25a0";

    public static readonly string ExportImportHeader = $"JSON Export/Import | [E] Export | [I] Import | [{ConfirmKey.ToString()}] Confirm | [{EndKey.ToString()}] Quit ";
}

public static class Utils {
    public static string MakeErrorMessage(string input) {
        return string.Concat(Constants.ErrorHeader, " ", input);
    }
}

