using System.Globalization;
using ExpenseManager.Data;
using ExpenseManager.IO;
using ExpenseManager.Models;

namespace ExpenseManager;

public class UserSession {
    private readonly DatabaseController _controller;
    private readonly User _user;
    private List<Expense> _expenses;
    private Dictionary<int, string> _categories;

    public UserSession(DatabaseController controller, User user) {
        _controller = controller;
        _user = user;
        _expenses = _controller.GetExpenses(user.Id);
        _categories = _controller.GetUserCategories(user.Id);
    }

    private decimal CalculateBalance() {
        Decimal sum = _expenses.Sum(e => e.Amount);
        return sum;
    }

    public async void RunSession() {
        UserPrinter printer = new UserPrinter(_user, _categories);
        int currentExpenseIndex = 0;
        string errString = "";
        while (true) {
            Console.Clear();
            printer.DisplayHeader(errString);
            errString = "";

            ECommands command = printer.UserSession(_expenses, ref currentExpenseIndex);
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
                    break;
                case ECommands.AddCategory:
                    string? categoryName = printer.AddCategory();
                    if (categoryName != null) {
                        await _controller.AddUserCategory(_user.Id, categoryName);
                        _categories = _controller.GetUserCategories(_user.Id);
                        printer._categories = _categories;
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