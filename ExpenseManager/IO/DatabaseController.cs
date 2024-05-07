using ExpenseManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.IO;

public class MyAppContext : DbContext {
    private string _connectionString;
    public DbSet<User> Users { get; set; }
    public DbSet<Expense> Expenses { get; set; }

    public MyAppContext(string connectionString) {
        _connectionString = connectionString;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite($"Data Source={_connectionString};");
    }
}

public class DatabaseController {
    private MyAppContext _db;

    public DatabaseController(string connectionString) {
        _db = new MyAppContext(connectionString);
    }

    public void Deconstruct() {
        _db.Dispose();
    }

    public async void AddUser(User user) {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public User? GetUser(string username) {
        return _db.Users.FirstOrDefault(u => u.Username == username);
    }

    public User? GetUser(int id) {
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public List<User> GetUsers() {
        return _db.Users.ToList();
    }

    public async void AddExpense(Expense expense) {
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
    }

    public List<Expense> GetExpenses(int id) {
        return _db.Expenses.Where(e => e.UserId == id).ToList();
    }
}