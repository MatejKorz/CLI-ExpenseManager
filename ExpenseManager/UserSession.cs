using System.Globalization;
using ExpenseManager.Data;
using ExpenseManager.IO;
using ExpenseManager.Models;

namespace ExpenseManager;

public class UserSession {
    private readonly DatabaseController _controller;
    private readonly User _user;
    private List<Expense> _expenses;
    private readonly Dictionary<int, string> _categories;
    private Filter _filter;

    public UserSession(DatabaseController controller, User user) {
        _controller = controller;
        _user = user;
        _expenses = _controller.GetExpenses(user.Id);
        _categories = _controller.GetUserCategories(user.Id).Result;
        _filter = new Filter(_categories);
    }

    private decimal CalculateBalance() {
        Decimal sum = _expenses.Sum(e => e.Amount);
        return sum;
    }

    public async void RunSession() {
        UserPrinter printer = new UserPrinter(_user, in _categories, ref _filter);
        int currentExpenseIndex = 0;
        string errString = "";
        while (true) {
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
                    var newFilter = printer.SetupFilter();
                    _filter = newFilter;
                    // must reset because numbers of expenses change
                    currentExpenseIndex = 0;
                    break;
                case ECommands.AddCategory:
                    string? categoryName = printer.AddCategory();
                    if (categoryName != null) {
                        await _controller.AddUserCategory(_user.Id, categoryName);
                        var newCategory = await _controller.GetUserCategory(_user.Id, categoryName);
                        if (newCategory != null) {
                            _categories[newCategory.Id] = newCategory.Name;
                        }
                        _filter.UpdateFilterCategory(_categories);
                    }
                    break;
                case ECommands.DisplayBalance:
                    errString = Constants.DisplayAmount.Replace(Constants.Replacable, CalculateBalance().ToString("0.00"));
                    break;
                case ECommands.Quit: return;
            }
        }
    }
}