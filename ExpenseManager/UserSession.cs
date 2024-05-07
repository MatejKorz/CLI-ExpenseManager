using ExpenseManager.Data;
using ExpenseManager.IO;
using ExpenseManager.Models;

namespace ExpenseManager;

public class UserSession {
    private readonly DatabaseController _controller;
    private readonly User _user;
    private List<Expense> _expenses;

    public UserSession(DatabaseController controller, User user) {
        _controller = controller;
        _user = user;
        _expenses = _controller.GetExpenses(user.Id);
    }

    public void RunSession() {
        int currentExpenseIndex = 0;
        string errString = "";
        while (true) {
            Console.Clear();
            Printer.DisplayHeader(_user.Username, errString);
            errString = "";

            ECommands command = Printer.UserSession(_expenses, ref currentExpenseIndex);
            switch (command) {
                case ECommands.Invalid: errString = "[ error ] invalid key";
                    break;
                case ECommands.AddExpense:
                    Expense? expense = Printer.AddExpense(_user.Id);
                    if (expense != null) {
                        _controller.AddExpense(expense);
                        _expenses.Add(expense);
                    }
                    break;
                case ECommands.Filter:
                    break;
                case ECommands.DisplayBalance:
                    break;
                case ECommands.Quit: return;
            }
        }
    }
}