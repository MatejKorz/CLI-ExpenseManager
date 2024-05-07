using System.Security;
using System.Text;
using ExpenseManager.Data;
using ExpenseManager.Models;
using Microsoft.Extensions.Primitives;

namespace ExpenseManager.IO;

public static class Printer {
    public static ECommands Startup() {
        string errString = "";
        while (true) {
            Console.Clear();
            Console.Write(errString);
            Console.Write(Constants.Startup);
            var key = Console.ReadKey();
            switch (key.Key) {
                case ConsoleKey.R: return ECommands.Register;
                case ConsoleKey.L: return ECommands.Login;
                case Constants.EndKey: return ECommands.Quit;
                default: errString = "[ error ] invalid key pressed"; break;
            }
        }
    }

    public static void DisplayHeader(string username, string error = "") {
        string header = Constants.Header;
        Console.WriteLine(header.Replace(Constants.Replacable, username));
        Console.WriteLine(error);
    }

    private static void DisplayExpense(Expense expense, bool selected = false) {
        string expStr = $"| {expense.Id, +4} | {expense.Amount, +10} | {expense.Category, +12} | {expense.DateTime, +16} | {expense.Description}";
        if (selected) {
            var foreground = Console.ForegroundColor;
            var background = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(expStr);
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
        } else {
            Console.WriteLine(expStr);
        }
    }

    private static void DisplayExpenses(List<Expense> expenses, int index) {
        var (topIndex, bottomIndex) = (index - Constants.ExpensePerScreen / 2, index + Constants.ExpensePerScreen / 2);
        if (expenses.Count < Constants.ExpensePerScreen) {
            topIndex = 0;
            bottomIndex = expenses.Count - 1;
        } else if (topIndex < 0) {
            bottomIndex += int.Abs(topIndex);
            topIndex = 0;
        } else if (bottomIndex >= expenses.Count) {
            topIndex = expenses.Count - Constants.ExpensePerScreen;
            bottomIndex = expenses.Count - 1;
        }

        for (int i = topIndex; i <= bottomIndex; ++i) {
            DisplayExpense(expenses[i], i == index);
        }
    }

    public static ECommands UserSession(List<Expense> expenses, ref int currIndex) {
        Console.WriteLine(Constants.ExpenseHeader);
        DisplayExpenses(expenses, currIndex);
        var key = Console.ReadKey();
        switch (key.Key) {
            case ConsoleKey.UpArrow : currIndex = int.Max(currIndex - 1, 0); break;
            case ConsoleKey.DownArrow : currIndex = int.Min(currIndex + 1, expenses.Count - 1); break;
            case ConsoleKey.A : return ECommands.AddExpense;
            case ConsoleKey.F : return ECommands.Filter;
            case Constants.EndKey : return ECommands.Quit;
            case ConsoleKey.D : return ECommands.DisplayBalance;
            default: return ECommands.Invalid;
        }

        return ECommands.Nothing;
    }

    private static void DisplayAddingExpense(string amount, Categories categories, string description, int active) {
        Console.WriteLine();
        var foreground = Console.ForegroundColor;
        var background = Console.BackgroundColor;
        for (int i = 0; i < 3; ++i) {
            if (i == active) {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
            }
            Console.Write(Constants.AddExpenseLines[i]);
            switch (i) {
                case 0: Console.WriteLine(amount);
                    break;
                case 1: Console.WriteLine(categories);
                    break;
                case 2: Console.WriteLine(description);
                    break;
            }
            if (i == active) {
                Console.BackgroundColor = background;
                Console.ForegroundColor = foreground;
            }
        }
    }

    public static Expense? AddExpense(int userId) {
        StringBuilder amountString = new StringBuilder();
        int categoryIndex = 0;
        var categories = (Categories[]) Enum.GetValues(typeof(Categories));
        StringBuilder description = new StringBuilder();
        int active = 0;
        string errString = "";

        while (true) {
            Console.Clear();
            Console.WriteLine(Constants.AddExpenseHeader);
            Console.WriteLine(errString);
            DisplayAddingExpense(amountString.ToString(), categories[categoryIndex], description.ToString(), active);
            ConsoleKeyInfo key = Console.ReadKey();
            switch (key.Key) {
                case ConsoleKey.UpArrow: active = int.Max(active - 1, 0);
                    break;
                case ConsoleKey.DownArrow: active = int.Min(active + 1, 2);
                    break;
                case ConsoleKey.RightArrow:
                    if (active == 1) {
                        categoryIndex = int.Min(categoryIndex + 1, categories.Length - 1);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (active == 1) {
                        categoryIndex = int.Max(categoryIndex - 1, 0);
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (active == 0 && amountString.Length != 0) {
                        amountString.Remove(amountString.Length - 1, 1);
                    } else if (active == 2 && description.Length != 0) {
                        description.Remove(description.Length - 1, 1);
                    }
                    break;
                case Constants.EndKey: return null;
                case ConsoleKey.Enter:
                    if (!decimal.TryParse(amountString.ToString(), out decimal amount)) {
                        errString = "[ error ] cannot parse amount";
                        break;
                    }

                    return new Expense(0, userId, decimal.Round(amount, 2), categories[categoryIndex], description.ToString(),
                        DateTime.Now.ToString(Constants.DateTimeFormat));
                default:
                    if (active == 0) {
                        amountString.Append(key.KeyChar);
                    } else if (active == 2) {
                        description.Append(key.KeyChar);
                    } else {
                        errString = "[ error ] invalid key";
                    } break;
            }
        }
    }

    public static string? UsernamePrompt(string previous) {
        Console.Clear();
        Console.Write(previous);
        Console.Write(Constants.Username);
        return Console.ReadLine();
    }

    public static SecureString? PasswordPrompt(string previous) {
        Console.Clear();
        Console.WriteLine(previous);
        Console.Write(Constants.Password);
        var passwordSb = new SecureString();
        ConsoleKeyInfo keyInfo;

        while (true) {
            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter) {
                break;
            }

            if (keyInfo.Key == ConsoleKey.Backspace) {
                if (passwordSb.Length > 0) {
                    passwordSb.RemoveAt(passwordSb.Length - 1);
                    Console.Write("\b \b");
                }
            } else {
                passwordSb.AppendChar(keyInfo.KeyChar);
                Console.Write('*');
            }
        }

        return passwordSb;
    }
}