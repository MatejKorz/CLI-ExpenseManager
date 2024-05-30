using System.Security;
using System.Security.Cryptography;
using ExpenseManager.Data;
using ExpenseManager.IO;
using ExpenseManager.Models;

namespace ExpenseManager.Login;

public class LoginManager {
    private DatabaseController _dbController;
    private StarterPrinter _starterPrinter;

    public LoginManager(DatabaseController dbController) {
        _dbController = dbController;
        _starterPrinter = new StarterPrinter();
    }

    public async void Register() {
        string errString = "";
        var users = await _dbController.GetUsers();

        while (true) {
            var (name, password) = _starterPrinter.UsernamePasswordPrompt(false, errString);
            if (name == null || password == null) { return; }

            var user = users.FirstOrDefault(u => u.Username == name);
            if (user != null) {
                errString = Utils.MakeErrorMessage("user already exists");
                continue;
            }
            _dbController.AddUser(new User(0, name, password));
            break;
        }
    }

    public async Task<int> Login() {
        string errString = "";
        var users = await _dbController.GetUsers();

        while (true) {
            var (name, password) = _starterPrinter.UsernamePasswordPrompt(true, errString);
            if (name == null || password == null) { return -1; }

            var user = users.FirstOrDefault(u => u.Username == name);
            if (user == null) {
                errString = Utils.MakeErrorMessage("user does not exist");
                continue;
            }

            if (Equals(password, user.Password)) {
                return user.Id;
            }
            errString = Utils.MakeErrorMessage("incorrect password");
        }
    }
}