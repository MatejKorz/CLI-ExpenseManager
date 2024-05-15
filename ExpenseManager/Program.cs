using ExpenseManager.IO;
using ExpenseManager.Login;
using ExpenseManager.Models;

namespace ExpenseManager;

class Program {
    static async Task Main(string[] args) {
        string databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.db");
        var databaseController = new DatabaseController(databasePath);
        var loginManager = new LoginManager(databaseController);
        int loggedUserId = -1;
        StarterPrinter mainStarterPrinter = new StarterPrinter();

        while (true) {
            Console.Clear();
            ECommands command = mainStarterPrinter.Startup();
            switch (command) {
                case ECommands.Register:
                    loginManager.Register();
                    break;
                case ECommands.Login:
                    loggedUserId = await loginManager.Login();
                    break;
                case ECommands.Quit:
                    // TODO free everything
                    databaseController.Deconstruct();
                    Console.Clear();
                    Console.Write("Goodbye");
                    return;
            }

            Console.Clear();
            if (loggedUserId == -1) {
                continue;
            }

            var loggedUser = databaseController.GetUser(loggedUserId);
            if (loggedUser == null) {
                loggedUserId = -1;
                continue;
            }
            var session = new UserSession(databaseController, loggedUser);
            session.RunSession();


            loggedUserId = -1;
        }
    }
}