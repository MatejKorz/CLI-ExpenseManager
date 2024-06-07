using System.Text;
using ExpenseManager.Data;
using ExpenseManager.Models;
using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.TickGenerators;

namespace ExpenseManager.IO;

public class UserPrinter {
    private User _user;
    private DatabaseController _dbControler;
    private Filter _filter;

    public UserPrinter(User user, ref Filter filter, DatabaseController controller) {
        _user = user;
        _filter = filter;
        _dbControler = controller;
    }

    public void DisplayHeader(string error = "") {
        string header = Constants.Header;
        Console.WriteLine(header.Replace(Constants.Replacable, _user.Username));
        Console.WriteLine(error);
    }

    private void DisplayExpense(Expense expense, Dictionary<int, string> categories,bool selected = false) {
        ConsoleColor expenseColor = expense.Amount > 0 ? ConsoleColor.DarkGreen : ConsoleColor.Red;
        string idString = $"| {expense.Id,+4} | ";
        string expenseString = $"{expense.Amount,+10:0.00}";
        string restString = $" | {categories[expense.CategoryId],+12} | {expense.DateTime,+16} | {expense.Description}";

        if (selected) {
            Highlighter.WriteLine(idString + expenseString + restString, ConsoleColor.Black, expenseColor);
        } else {
            Highlighter.Write(idString);
            Highlighter.Write(expenseString, expenseColor);
            Highlighter.WriteLine(restString);
        }
        Console.ResetColor();
    }

    private async void DisplayExpenses(List<Expense> expenses, int index) {
        var expensePerScreen = Console.WindowHeight - 4; // header + errorLine + tab header
        var categories = await _dbControler.GetUserCategories(_user.Id);
        var (topIndex, bottomIndex) = (index - expensePerScreen / 2, index + expensePerScreen / 2);
        if (expenses.Count < expensePerScreen) {
            topIndex = 0;
            bottomIndex = expenses.Count;
        } else if (topIndex < 0) {
            bottomIndex += int.Abs(topIndex);
            topIndex = 0;
        } else if (bottomIndex >= expenses.Count) {
            topIndex = expenses.Count - expensePerScreen;
            bottomIndex = expenses.Count;
        }

        for (int i = topIndex; i < bottomIndex; ++i) {
            DisplayExpense(expenses[i], categories, i == index);
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
            case ConsoleKey.J : return ECommands.ExportImport;
            case ConsoleKey.S : return ECommands.Statistics;
            default: return ECommands.Invalid;
        }

        return ECommands.Nothing;
    }

    private void DisplayAddingExpense(string amount, string category, string description, string dateTime, int active) {
        string[] fields = { amount, category, description, dateTime};
        for (int i = 0; i < Constants.AddExpenseLines.Count; ++i) {
            Highlighter.Write(Constants.AddExpenseLines[i], active == i ? ConsoleColor.Black : null, active == i ? ConsoleColor.White : null);
            Highlighter.WriteLine(fields[i], active == i ? ConsoleColor.Black : null, active == i ? ConsoleColor.White : null);
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
        var categories = _dbControler.GetUserCategories(_user.Id).Result.OrderBy(c => c.Value).ToDictionary();

        while (true) {
            Console.Clear();
            Console.WriteLine(income ? Constants.AddIncomeHeader : Constants.AddExpenseHeader);
            Console.WriteLine(errString);
            DisplayAddingExpense(amountString.ToString(), categories.ElementAt(categoryIndex).Value, description.ToString(), dateTimeString.ToString(), activeIndex);
            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key) {
                case ConsoleKey.UpArrow: activeIndex = int.Max(activeIndex - 1, 0);
                    break;
                case ConsoleKey.DownArrow: activeIndex = int.Min(activeIndex + 1, options.Count - 1);
                    break;
                case ConsoleKey.RightArrow:
                    if (activeIndex == 1) {
                        categoryIndex = int.Min(categoryIndex + 1, categories.Count - 1);
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

                    return new Expense(0, _user.Id, amount, categories.ElementAt(categoryIndex).Key, description.ToString(),
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

    public async Task<string?> AddCategory() {
        StringBuilder categoryName = new StringBuilder();
        string errString = "";
        var categories = await _dbControler.GetUserCategories(_user.Id);

        while (true) {
            Console.Clear();
            Console.WriteLine(Constants.AddCategoryHeader);
            Console.WriteLine(errString);
            Console.Write("Your new category: ");
            Console.WriteLine(categoryName.ToString());
            var key = Console.ReadKey();
            switch (key.Key) {
                case Constants.EndKey : return null;
                case Constants.ConfirmKey :
                    if (categoryName.ToString().Contains(' ')) {
                        errString = Utils.MakeErrorMessage("category can not contain space");
                        break;
                    }
                    if (categories.ContainsValue(categoryName.ToString())) {
                        errString = Utils.MakeErrorMessage("category already present");
                        break;
                    }
                    return categoryName.ToString();
                case ConsoleKey.Backspace :
                    if (categoryName.Length > 0) {
                        categoryName.Remove(categoryName.Length - 1, 1);
                    }
                    break;
                default:
                    categoryName.Append(key.KeyChar);
                    break;
            }
        }
    }

    private void DisplayFilterCategory(int id, string name, bool selected = false) {
        string symbol = _filter.Categories[id] ? Constants.FilterCategorySelected : Constants.FilterCategoryNotSelected;
        ConsoleColor symbolColor = _filter.Categories[id] ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;

        if (selected) {
            Highlighter.Write($"{name} ", ConsoleColor.Black, ConsoleColor.White);
            Highlighter.WriteLine($"{symbol}", symbolColor, ConsoleColor.White);
        } else {
            Highlighter.Write($"{name} ");
            Highlighter.WriteLine($"{symbol}", symbolColor);
        }
    }
    private void DisplayFilterCategories(int index, Dictionary<int, string> categories) {
        // dates are displayed above categories so not always should category be highlighted
        var categoriesPerScreen = Console.WindowHeight - 5; // header + errLine + 2*date
        int correctedIndex = int.Max(index, 0);
        var (topIndex, bottomIndex) = (correctedIndex - categoriesPerScreen / 2, correctedIndex + categoriesPerScreen / 2);
        if (categories.Count < categoriesPerScreen) {
            topIndex = 0;
            bottomIndex = categories.Count;
        } else if (topIndex < 0) {
            bottomIndex += int.Abs(topIndex);
            topIndex = 0;
        } else if (bottomIndex >= categories.Count) {
            topIndex = categories.Count - categoriesPerScreen;
            bottomIndex = categories.Count;
        }

        for (int i = topIndex; i < bottomIndex; ++i) {
            var (id, name) = categories.ElementAt(i);
            DisplayFilterCategory(id, name, i == index);
        }
    }

    public async Task<Filter> SetupFilter() {
        string errString = "";
        var datetimeFromString = new StringBuilder(_filter.DateTimeFrom.GetValueOrDefault().ToString(Constants.DateTimeFormat));
        var datetimeToString = new StringBuilder(_filter.DateTimeTo.GetValueOrDefault(DateTime.Now).ToString(Constants.DateTimeFormat));
        int activeIndex = 0;
        var originalFilter = new Filter(_filter);
        var categories = await _dbControler.GetUserCategories(_user.Id);

        while (true) {
            Console.Clear();
            Console.WriteLine(Constants.FilterHeader);
            Console.WriteLine(errString);
            errString = "";
            Highlighter.WriteLine($"Date FROM: {datetimeFromString}", activeIndex == 0 ? ConsoleColor.Black : null, activeIndex == 0 ? ConsoleColor.White: null);
            Highlighter.WriteLine($"Date TO:   {datetimeToString}", activeIndex == 1 ? ConsoleColor.Black : null, activeIndex == 1 ? ConsoleColor.White: null);
            DisplayFilterCategories(activeIndex - 2, categories);

            var key = Console.ReadKey();
            switch (key.Key) {
                case Constants.EndKey:
                    _filter = originalFilter;
                    return _filter;
                case Constants.ConfirmKey:
                    DateTime tmp;
                    if (datetimeFromString.Length == 0) {
                        _filter.DateTimeFrom = null;
                    } else if (DateTime.TryParse(datetimeFromString.ToString(), out tmp)) {
                        _filter.DateTimeFrom = tmp;
                    } else {
                        errString = Utils.MakeErrorMessage("invalid FROM date");
                        break;
                    }

                    if (datetimeToString.Length == 0) {
                        _filter.DateTimeTo = null;
                    } else if (DateTime.TryParse(datetimeToString.ToString(), out tmp)) {
                        _filter.DateTimeTo = tmp;
                    } else {
                        errString = Utils.MakeErrorMessage("invalid TO date");
                        break;
                    }

                    return _filter;
                case ConsoleKey.UpArrow : activeIndex = int.Max(activeIndex - 1, 0); break;
                case ConsoleKey.DownArrow : activeIndex = int.Min(activeIndex + 1, categories.Count + 2 - 1); break;
                case ConsoleKey.P :
                    if (activeIndex >= 2) {
                        var (id, value) = _filter.Categories.ElementAt(activeIndex - 2);
                        _filter.Categories[id] = !value;
                    }
                    break;
                case ConsoleKey.R : _filter.ResetFilter();
                    datetimeFromString = new StringBuilder(_filter.DateTimeFrom.GetValueOrDefault().ToString(Constants.DateTimeFormat));
                    datetimeToString = new StringBuilder(_filter.DateTimeTo.GetValueOrDefault(DateTime.Now).ToString(Constants.DateTimeFormat));
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

    private string MakeGroupString(Expense e) {
        string ret = DateTime.Parse(e.DateTime).Year.ToString() + ',' + DateTime.Parse(e.DateTime).Month;
        return ret;
    }
    public string Statistics(List<Expense> expenses, string username) {
        var groupByMonth = expenses.GroupBy(MakeGroupString).OrderBy(g => g.Key).ToDictionary(g => g.Key);

        Dictionary<string, ValueTuple<decimal, decimal>> vals = new Dictionary<string, (decimal, decimal)>();
        foreach (var (key, exps) in groupByMonth) {
            decimal amountExpense = 0m;
            decimal amountIncome = 0m;
            foreach (var exp in exps) {
                if (exp.Amount > 0) {
                    amountIncome += exp.Amount;
                } else {
                    amountExpense += decimal.Abs(exp.Amount);
                }
            }
            vals[key] = (amountExpense, amountIncome);
        }

        string[] names = vals.Keys.ToArray();
        decimal[] expsArray = vals.Values.Select(v => v.Item1).ToArray();
        decimal[] incArray = vals.Values.Select(v => v.Item2).ToArray();

        var plt = new Plot();

        // Define the palette
        Category10 palette = new Category10();

        // Create bar plots for each set of values
        List<Bar> bars = new List<Bar>();
        for (int i = 0; i < names.Length; i++)
        {
            bars.Add(new Bar
            {
                Position = i * 3 + 1,
                Value = (double)expsArray[i],
                FillColor = palette.GetColor(3)
            });

            bars.Add(new Bar
            {
                Position = i * 3 + 2,
                Value = (double)incArray[i],
                FillColor = palette.GetColor(2)
            });
        }

        // Add the bars to the plot
        plt.Add.Bars(bars.ToArray());

        // Build the legend manually
        plt.Legend.IsVisible = true;
        plt.Legend.Alignment = Alignment.UpperLeft;
        plt.Legend.ManualItems.Add(new() { LabelText = "Expenses", FillColor = palette.GetColor(3) });
        plt.Legend.ManualItems.Add(new() { LabelText = "Incomes", FillColor = palette.GetColor(2) });

        // Show group labels on the bottom axis
        Tick[] ticks = names.Select((c, i) => new Tick(i * 3 + 1.5, c)).ToArray();
        plt.Axes.Bottom.TickGenerator = new NumericManual(ticks);
        plt.Axes.Bottom.MajorTickStyle.Length = 0;
        plt.HideGrid();

        // Tell the plot to autoscale with no padding beneath the bars
        plt.Axes.Margins(bottom: 0);

        // Save the plot as an image file
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"{username}_expenseGraph.png");
        plt.SavePng(filePath, 800, 600);
        return filePath;
    }

    public ValueTuple<string, bool>? ExportImport(string username) {
        StringBuilder filepath = new StringBuilder();
        int activeIndex = 0;
        bool export = true;
        string errString = "";
        while (true) {
            Console.Clear();
            Console.WriteLine(Constants.ExportImportHeader);
            Console.WriteLine(errString);
            string status = export ? "EXPORT" : "IMPORT";
            Highlighter.WriteLine($"Status: {status}", activeIndex == 0 ? ConsoleColor.Black : null, activeIndex == 0 ? ConsoleColor.White: null);
            Highlighter.WriteLine($"Filepath: {filepath}", activeIndex == 1 ? ConsoleColor.Black : null, activeIndex == 1 ? ConsoleColor.White: null);
            var key = Console.ReadKey();
            switch (key.Key) {
                case Constants.EndKey : return null;
                case Constants.ConfirmKey :
                    if (export) {
                        if (Directory.Exists(filepath.ToString())) {
                            filepath = new StringBuilder(Path.Combine(filepath.ToString(), $"{username}_export.json"));
                        }

                        if (File.Exists(filepath.ToString())) {
                            Console.Write($"Overwrite file {filepath}? Y/n: ");
                            if (Console.ReadKey().Key != ConsoleKey.Y) {
                                Console.WriteLine("aborting");
                                return null;
                            }
                            return (filepath.ToString(), export);
                        }
                    }
                    if (!export) {
                        if (!File.Exists(filepath.ToString())) {
                            errString = Utils.MakeErrorMessage("file does not exist");
                            break;
                        }

                        if (Path.GetExtension(filepath.ToString()).Equals("json", StringComparison.OrdinalIgnoreCase)) {
                            errString = Utils.MakeErrorMessage("file is not json");
                            break;
                        }

                        return (filepath.ToString(), export);
                    }

                    return (filepath.ToString(), export);
                case ConsoleKey.UpArrow : activeIndex = int.Max(0, activeIndex - 1);
                    break;
                case ConsoleKey.DownArrow : activeIndex = int.Min(1, activeIndex + 1);
                    break;
                case ConsoleKey.RightArrow :
                    if (activeIndex == 0) {
                        export = !export;
                    }
                    break;
                case ConsoleKey.LeftArrow :
                    if (activeIndex == 0) {
                        export = !export;
                    }
                    break;
                case ConsoleKey.Backspace :
                    if (activeIndex == 0) {
                        errString = Utils.MakeErrorMessage("invalid key");
                        break;
                    }

                    if (filepath.Length > 0) {
                        filepath.Remove(filepath.Length - 1, 1);
                    }
                    break;
                default:
                    // TODO characters to constants
                    if (activeIndex != 1 || !char.IsLetterOrDigit(key.KeyChar) && !"/.-_".Contains(key.KeyChar)) {
                        errString = Utils.MakeErrorMessage("invalid key");
                        break;
                    }
                    filepath.Append(key.KeyChar);
                    break;
            }
        }
    }
}