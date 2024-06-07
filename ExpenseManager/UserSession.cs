using ExpenseManager.Data;
using ExpenseManager.IO;
using ExpenseManager.Models;

namespace ExpenseManager;

public class UserSession {
    private readonly DatabaseController _controller;
    private readonly User _user;
    private List<Expense> _expenses;
    private Filter _filter;

    public UserSession(DatabaseController controller, User user) {
        _controller = controller;
        _user = user;
        _expenses = _controller.GetExpenses(user.Id);
        _filter = new Filter(_controller.GetUserCategories(_user.Id).Result);
    }

    private decimal CalculateBalance() {
        Decimal sum = _expenses.Sum(e => e.Amount);
        return sum;
    }

    public async Task RunSession() {
        UserPrinter printer = new UserPrinter(_user, ref _filter, _controller);
        int currentExpenseIndex = 0;
        string errString = "";
        while (true) {
            Console.Clear();
            Console.Clear();
            printer.DisplayHeader(errString);
            errString = "";
            var displayedExpenses = _expenses.Where(e => _filter.ExpenseBelongs(e)).ToList();
            ECommands command = printer.UserSession(displayedExpenses, ref currentExpenseIndex);
            Expense? expense;
            switch (command) {
                case ECommands.Invalid: errString = Utils.MakeErrorMessage("invalid key");
                    break;
                case ECommands.AddIncome:
                    expense = printer.AddExpense(true);
                    if (expense != null) {
                        _controller.AddExpense(expense);
                        _expenses.Add(expense);
                    }
                    break;
                case ECommands.AddExpense:
                    expense = printer.AddExpense();
                    if (expense != null) {
                        _controller.AddExpense(expense);
                        _expenses.Add(expense);
                    }
                    break;
                case ECommands.Filter:
                    _filter.UpdateFilterCategory(_controller.GetUserCategories(_user.Id).Result);
                    var newFilter = printer.SetupFilter();
                    _filter = newFilter.Result;
                    // must reset because numbers of expenses change
                    currentExpenseIndex = 0;
                    break;
                case ECommands.AddCategory:
                    var name = await printer.AddCategory();
                    if (name != null) {
                        await _controller.AddUserCategory(_user.Id, name);
                    }
                    _filter.UpdateFilterCategory(_controller.GetUserCategories(_user.Id).Result);
                    break;
                case ECommands.DisplayBalance:
                    errString = Constants.DisplayAmount.Replace(Constants.Replacable, CalculateBalance().ToString("0.00"));
                    break;
                case ECommands.ExportImport:
                    var ret = printer.ExportImport(_user.Username);
                    if (!ret.HasValue) {
                        break;
                    }
                    var (filepath, isExport) = ret.Value;
                    if (isExport) {
                        var exporter = new JsonController();
                        var categories = await _controller.GetUserCategories(_user.Id);
                        errString = await exporter.Serialize(filepath, _expenses, categories);
                    } else {
                        var importer = new JsonController();
                        errString = await importer.Deserialize(_user.Id, filepath, _controller);
                        _expenses = _controller.GetExpenses(_user.Id);
                    }
                    _filter.UpdateFilterCategory(_controller.GetUserCategories(_user.Id).Result);
                    break;
                case ECommands.Statistics:
                    errString = "[ export ] graph exported to " + printer.Statistics(_expenses, _user.Username);
                    break;
                case ECommands.Quit:
                    Console.Clear();
                    Console.Clear();
                    Highlighter.Write("Do you want to quit? [y/n]: ", ConsoleColor.Black, ConsoleColor.White);
                    if (Console.ReadKey().Key == ConsoleKey.Y) {
                        return;
                    }
                    break;
            }
        }
    }
}