using System.Security;
using System.Security.Cryptography;
using ExpenseManager.Data;
using ExpenseManager.IO;
using ExpenseManager.Models;

namespace ExpenseManager.Login;

public class LoginManager {
    private DatabaseController _dbController;

    public LoginManager(DatabaseController dbController) {
        _dbController = dbController;
    }

    public void Register() {
        string? username;
        SecureString? password;
        string errString = "";
        while (true) {
            username = Printer.UsernamePrompt(errString);
            if (username == null) { continue; }

            if (_dbController.GetUser(username) != null) {
                errString = "[ error ] user does not exist";
                continue;
            }

            break;
        }

        errString = "";
        while (true) {
            password = Printer.PasswordPrompt(errString + Constants.Username + username);
            if (password == null || password.Length < 5) {
                errString = "[ error ] password too weak";
                continue;
            }

            break;
        }

        byte[] hashedPassword = PasswordManager.HashSecureString(password);

        _dbController.AddUser(new User(0, username, Convert.ToBase64String(hashedPassword)));
    }

    public int Login() {
        string? username;
        SecureString? password;
        User? user;
        string errString = "";

        while (true) {
            username = Printer.UsernamePrompt(errString);
            if (username == null || (user = _dbController.GetUser(username)) == null || user.Id == null) {
                errString = "[ error ] user does not exist";
                continue;
            }

            break;
        }

        var cmpHash = user.Password;
        errString = "";
        while (true) {
            password = Printer.PasswordPrompt(errString + Constants.Username + username);
            errString = "[ error ] password incorrect";

            if (password != null && PasswordManager.ComparePasswords(password, cmpHash)) {
                break;
            }
        }

        return user.Id;
    }
}