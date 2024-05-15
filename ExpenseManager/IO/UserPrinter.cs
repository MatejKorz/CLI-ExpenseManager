using System.Text;
using ExpenseManager.Data;
using ExpenseManager.Models;

namespace ExpenseManager.IO;

public class UserPrinter {
    private User _user;
    private Dictionary<int, string> _categories;
    private Filter _filter;

    public UserPrinter(User user, in Dictionary<int, string> categories, ref Filter filter) {
        _user = user;
        _categories = categories;
        _filter = filter;

    }

    public void DisplayHeader(string error = "") {
        string header = Constants.Header;
        Console.WriteLine(header.Replace(Constants.Replacable, _user.Username));
        Console.WriteLine(error);
    }

    private void DisplayExpense(Expense expense, bool selected = false) {
        var foreground = Console.ForegroundColor;
        var background = Console.BackgroundColor;
        if (selected) {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = expense.Amount > 0 ? ConsoleColor.DarkGreen : ConsoleColor.Red;
        }

        Console.Write($"| {expense.Id, +4} | ");
        var foregroundInside = Console.ForegroundColor;
        if (!selected) {
            Console.ForegroundColor = expense.Amount > 0 ? ConsoleColor.DarkGreen : ConsoleColor.Red;
        }
        // TODO - FORMATING
        Console.Write($"{expense.Amount, +10:0.00}");
        Console.ForegroundColor = foregroundInside;
        Console.WriteLine($" | {_categories[expense.CategoryId], +12} | {expense.DateTime, +16} | {expense.Description}");
        if (selected) {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
        }
    }

    private void DisplayExpenses(List<Expense> expenses, int index) {
        var (topIndex, bottomIndex) = (index - Constants.ExpensePerScreen / 2, index + Constants.ExpensePerScreen / 2);
        if (expenses.Count < Constants.ExpensePerScreen) {
            topIndex = 0;
            bottomIndex = expenses.Count;
        } else if (topIndex < 0) {
            bottomIndex += int.Abs(topIndex);
            topIndex = 0;
        } else if (bottomIndex >= expenses.Count) {
            topIndex = expenses.Count - Constants.ExpensePerScreen;
            bottomIndex = expenses.Count;
        }

        for (int i = topIndex; i < bottomIndex; ++i) {
            DisplayExpense(expenses[i], i == index);
        }
    }

    public ECommands UserSession(List<Expense> expenses, ref int currIndex) {
        Console.WriteLine(Constants.ExpenseHeader);
        DisplayExpenses(expenses, currIndex);
        var key = Console.ReadKey();
        switch (key.Key) {
            case ConsoleKey.UpArrow : currIndex = int.Max(currIndex - 1, 0); break;
            case ConsoleKey.DownArrow : currIndex = int.Min(currIndex + 1, expenses.Count - 1); break;
            case ConsoleKey.A : return ECommands.AddExpense;
            case ConsoleKey.I : return ECommands.AddIncome;
            case ConsoleKey.F : return ECommands.Filter;
            case Constants.EndKey : return ECommands.Quit;
            case ConsoleKey.D : return ECommands.DisplayBalance;
            case ConsoleKey.C : return ECommands.AddCategory;
            default: return ECommands.Invalid;
        }

        return ECommands.Nothing;
    }

    private void DisplayAddingExpense(string amount, string category, string description, string dateTime, int active) {
        var foreground = Console.ForegroundColor;
        var background = Console.BackgroundColor;
        for (int i = 0; i < Constants.AddExpenseLines.Count; ++i) {
            if (i == active) {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
            }
            Console.Write(Constants.AddExpenseLines[i]);
            switch (i) {
                case 0: Console.WriteLine(amount);
                    break;
                case 1: Console.WriteLine(category);
                    break;
                case 2: Console.WriteLine(description);
                    break;
                case 3: Console.WriteLine(dateTime);
                    break;
            }
            if (i == active) {
                Console.BackgroundColor = background;
                Console.ForegroundColor = foreground;
            }
        }
    }

    public Expense? AddExpense(bool income = false) {
        StringBuilder amountString = new StringBuilder();
        StringBuilder description = new StringBuilder();
        StringBuilder dateTimeString = new StringBuilder(DateTime.Now.ToString(Constants.DateTimeFormat));
        int activeIndex = 0;
        int categoryIndex = 0;
        string errString = "";
        var options = new List<StringBuilder?> { amountString, null, description, dateTimeString };
        _categories = _categories.OrderBy(c => c.Value).ToDictionary();

        while (true) {
            Console.Clear();
            Console.WriteLine(income ? Constants.AddIncomeHeader : Constants.AddExpenseHeader);
            Console.WriteLine(errString);
            DisplayAddingExpense(amountString.ToString(), _categories.ElementAt(categoryIndex).Value, description.ToString(), dateTimeString.ToString(), activeIndex);
            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key) {
                case ConsoleKey.UpArrow: activeIndex = int.Max(activeIndex - 1, 0);
                    break;
                case ConsoleKey.DownArrow: activeIndex = int.Min(activeIndex + 1, options.Count - 1);
                    break;
                case ConsoleKey.RightArrow:
                    if (activeIndex == 1) {
                        categoryIndex = int.Min(categoryIndex + 1, _categories.Count - 1);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (activeIndex == 1) {
                        categoryIndex = int.Max(categoryIndex - 1, 0);
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (options[activeIndex] != null && options[activeIndex].Length > 0) {
                        options[activeIndex].Remove(options[activeIndex].Length - 1, 1);
                    }
                    break;
                case Constants.EndKey: return null;
                case Constants.ConfirmKey:
                    if (!decimal.TryParse(amountString.ToString(), out decimal amount)) {
                        errString = Utils.MakeErrorMessage("cannot parse amount");
                        break;
                    }

                    amount = decimal.Round(amount, 2);
                    amount = income ? decimal.Abs(amount) : -decimal.Abs(amount);
                    DateTime dateTime;
                    if (!DateTime.TryParse(dateTimeString.ToString(), out dateTime)) {
                        errString = Utils.MakeErrorMessage("cannot parse date");
                        break;
                    }

                    return new Expense(0, _user.Id, amount, _categories.ElementAt(categoryIndex).Key, description.ToString(),
                        dateTime.ToString(Constants.DateTimeFormat));
                default:
                    if (options[activeIndex] != null) {
                        options[activeIndex].Append(key.KeyChar);
                    } else {
                        errString = Utils.MakeErrorMessage("invalid key");
                    } break;
            }
        }
    }

    public string? AddCategory() {
        StringBuilder categoryName = new StringBuilder();
        string errString = "";

        while (true) {
            Console.Clear();
            Console.WriteLine(Constants.AddCategoryHeader);
            Console.WriteLine(errString);
            Console.WriteLine(categoryName.ToString());
            var key = Console.ReadKey();
            switch (key.Key) {
                case Constants.EndKey : return null;
                case Constants.ConfirmKey :
                    if (categoryName.ToString().Contains(' ')) {
                        errString = Utils.MakeErrorMessage("category can not contain space");
                        break;
                    }
                    if (_categories.ContainsValue(categoryName.ToString())) {
                        errString = Utils.MakeErrorMessage("category already present");
                        break;
                    }
                    return categoryName.ToString();
                case ConsoleKey.Backspace : categoryName.Remove(categoryName.Length - 1, 1); break;
                default:
                    categoryName.Append(key.KeyChar);
                    break;
            }
        }
    }

    private void DisplayFilterCategory(int id, string name, bool selected = false) {
        var foreground = Console.ForegroundColor;
        var background = Console.BackgroundColor;
        if (selected) {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
        }
        Console.Write($"{name} ");
        string symbol;
        var foregoundInner = Console.ForegroundColor;
        if (_filter.Categories[id]) {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            symbol = Constants.FilterCategorySelected;
        } else {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            symbol = Constants.FilterCategoryNotSelcted;
        }
        Console.WriteLine(symbol);
        Console.ForegroundColor = foregoundInner;

        if (selected) {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
        }
    }
    private void DisplayFilterCategories(int index) {
        // dates are displayed above categories so not always should category be highlighted
        int correctedIndex = int.Max(index, 0);
        var (topIndex, bottomIndex) = (correctedIndex - Constants.CategoriesPerScreen / 2, correctedIndex + Constants.CategoriesPerScreen / 2);
        if (_categories.Count < Constants.CategoriesPerScreen) {
            topIndex = 0;
            bottomIndex = _categories.Count;
        } else if (topIndex < 0) {
            bottomIndex += int.Abs(topIndex);
            topIndex = 0;
        } else if (bottomIndex >= _categories.Count) {
            topIndex = _categories.Count - Constants.CategoriesPerScreen;
            bottomIndex = _categories.Count;
        }

        for (int i = topIndex; i < bottomIndex; ++i) {
            var (id, name) = _categories.ElementAt(i);
            DisplayFilterCategory(id, name, i == index);
        }
    }

    public Filter SetupFilter() {
        string errString = "";
        var datetimeFromString = new StringBuilder();
        var datetimeToString = new StringBuilder();
        int activeIndex = 0;
        var originalFilter = _filter;

        while (true) {
            Console.Clear();
            Console.WriteLine(Constants.FilterHeader);
            Console.WriteLine(errString);
            Console.WriteLine($"Date FROM: {datetimeFromString}");
            Console.WriteLine($"Date TO:   {datetimeToString}");
            DisplayFilterCategories(activeIndex - 2);

            var key = Console.ReadKey();
            switch (key.Key) {
                case Constants.EndKey:
                    _filter = originalFilter;
                    return _filter;
                case Constants.ConfirmKey: return _filter;
                case ConsoleKey.UpArrow : activeIndex = int.Max(activeIndex - 1, 0); break;
                case ConsoleKey.DownArrow : activeIndex = int.Min(activeIndex + 1, _categories.Count + 2 - 1); break;
                case ConsoleKey.P :
                    if (activeIndex >= 2) {
                        var (id, value) = _filter.Categories.ElementAt(activeIndex - 2);
                        _filter.Categories[id] = !value;
                    }
                    break;
                case ConsoleKey.Backspace :
                    if (activeIndex == 0 && datetimeFromString.Length > 0) {
                        datetimeFromString.Remove(datetimeFromString.Length - 1, 1);
                    } else if (activeIndex == 1 && datetimeToString.Length > 0) {
                        datetimeToString.Remove(datetimeToString.Length - 1, 1);
                    } else {
                        errString = Utils.MakeErrorMessage("invalid key");
                    }
                    break;
                default:
                    if (activeIndex == 0) {
                        datetimeFromString.Append(key.KeyChar);
                    } else if (activeIndex == 1) {
                        datetimeToString.Append(key.KeyChar);
                    } else {
                        errString = Utils.MakeErrorMessage("invalid key");
                    }
                    break;
            }
        }
    }
}