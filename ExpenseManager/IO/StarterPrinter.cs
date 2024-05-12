using System.Security;
using System.Text;
using ExpenseManager.Data;
using ExpenseManager.Login;
using ExpenseManager.Models;

namespace ExpenseManager.IO;

public class StarterPrinter {
    public ECommands Startup() {
        string errString = "";
        while (true) {
            Console.Clear();
            Console.WriteLine(Constants.StartupHeader);
            Console.WriteLine(errString);
            Console.WriteLine(Constants.StartupInfo);
            var key = Console.ReadKey();
            switch (key.Key) {
                case ConsoleKey.R: return ECommands.Register;
                case ConsoleKey.L: return ECommands.Login;
                case Constants.EndKey: return ECommands.Quit;
                default:
                    errString = Utils.MakeErrorMessage("invalid key pressed");
                    break;
            }
        }
    }

    public ValueTuple<string?, string?> UsernamePasswordPrompt(bool login, string previousError = "") {
        var passwrdSecString = new SecureString();
        var userStringBuild = new StringBuilder();
        int active = 0;
        string errString = previousError;

        while (true) {
            Console.Clear();
            Console.WriteLine(login ? Constants.LoginHeader : Constants.RegisterHeader);
            Console.WriteLine(errString);
            var foreground = Console.ForegroundColor;
            var background = Console.BackgroundColor;
            if (active == 0) {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.WriteLine($"Username: {userStringBuild}");
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            if (active == 1) {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.WriteLine($"Password: {new string('*', passwrdSecString.Length)}");
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;

            errString = "";
            var key = Console.ReadKey();
            switch (key.Key) {
                case Constants.EndKey: return (null, null);
                case ConsoleKey.UpArrow:
                    active = int.Max(0, active - 1);
                    break;
                case ConsoleKey.DownArrow:
                    active = int.Min(1, active + 1);
                    break;
                case ConsoleKey.Enter:
                    if (userStringBuild.Length < 1 || passwrdSecString.Length < 1) {
                        errString = Utils.MakeErrorMessage("empty fields");
                        break;
                    }
                    var passHash = PasswordManager.HashSecureString(passwrdSecString);
                    return (userStringBuild.ToString(), Convert.ToBase64String(passHash));
                case ConsoleKey.Backspace:
                    if (active == 0 && userStringBuild.Length > 0) {
                        userStringBuild.Remove(userStringBuild.Length - 1, 1);
                    } else if (active == 1 && passwrdSecString.Length > 0) {
                        passwrdSecString.RemoveAt(passwrdSecString.Length - 1);
                    }

                    break;
                default:
                    if (active == 0) {
                        if (!char.IsLetterOrDigit(key.KeyChar)) {
                            errString = Utils.MakeErrorMessage("only alphanumeric characters allowed");
                            break;
                        }
                        userStringBuild.Append(key.KeyChar);
                    } else if (active == 1) {
                        passwrdSecString.AppendChar(key.KeyChar);
                    }
                    break;
            }
        }
    }
}