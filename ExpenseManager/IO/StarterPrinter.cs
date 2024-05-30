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

            string username = $"Username: {userStringBuild}";
            if (active == 0) {
                Highlighter.WriteLine(username, ConsoleColor.Black, ConsoleColor.White);
            } else {
                Highlighter.WriteLine(username);
            }

            string password = $"Password: {new string('*', passwrdSecString.Length)}";
            if (active == 1) {
                Highlighter.WriteLine(password, ConsoleColor.Black, ConsoleColor.White);
            } else {
                Highlighter.WriteLine(password);
            }

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
                case Constants.ConfirmKey:
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
                    if (!char.IsLetterOrDigit(key.KeyChar) && !Constants.AllowedChars.Contains(key.KeyChar)) {
                        errString = Utils.MakeErrorMessage($"only alphanumeric and {Constants.AllowedChars} chars allowed");
                        break;
                    }
                    if (active == 0) {
                        userStringBuild.Append(key.KeyChar);
                    } else if (active == 1) {
                        passwrdSecString.AppendChar(key.KeyChar);
                    }
                    break;
            }
        }
    }
}