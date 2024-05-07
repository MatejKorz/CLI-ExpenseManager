using System.Globalization;
using ExpenseManager.IO;
using ExpenseManager.Login;
using ExpenseManager.Models;

namespace ExpenseManager;

class Program {
    static void Main(string[] args) {
        var databaseController = new DatabaseController("./data.db");
        var loginManager = new LoginManager(databaseController);
        int? loggedUserId = null;

        while (true) {
            Console.Clear();
            ECommands command = Printer.Startup();
            switch (command) {
                case ECommands.Register:
                    loginManager.Register();
                    break;
                case ECommands.Login:
                    loggedUserId = loginManager.Login();
                    break;
                case ECommands.Quit:
                    // TODO free everything
                    databaseController.Deconstruct();
                    Console.Clear();
                    Console.Write("Goodbye");
                    return;
            }

            Console.Clear();
            if (loggedUserId == null) {
                continue;
            }

            var loggedUser = databaseController.GetUser(loggedUserId.Value);
            if (loggedUser == null) {
                loggedUserId = null;
                continue;
            }
            var session = new UserSession(databaseController, loggedUser);
            session.RunSession();


            loggedUserId = null;
        }
    }
}