using ExpenseManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ExpenseManager.IO;

public class MyAppContext : DbContext {
    private string _connectionString;
    public DbSet<User> Users { get; set; }
    public DbSet<Expense> Expenses { get; set; }

    public DbSet<Category> Categories { get; set; }

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
        _db.SaveChanges();
        _db.Dispose();
    }

    public async void AddUser(User user) {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public User? GetUser(int id) {
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<List<User>> GetUsers() {
        return await _db.Users.ToListAsync();
    }

    public async Task<Category?> GetUserCategory(int id, string name) {
        return await _db.Categories.Where(c => c.Name == name && (c.UserId == id || c.UserId == 0)).FirstOrDefaultAsync();
    }

    public async Task<Dictionary<int, string>> GetUserCategories(int userId) {
        return await _db.Categories.Where(c => c.UserId == userId || c.UserId == 0).ToDictionaryAsync(c => c.Id, c => c.Name);
    }

    public async Task AddUserCategory(int userId, string categoryName) {
        _db.Categories.Add(new Category(0, userId, categoryName));
        await _db.SaveChangesAsync();
    }

    public async void AddExpense(Expense expense) {
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
    }

    public List<Expense> GetExpenses(int id) {
        return _db.Expenses.Where(e => e.UserId == id).ToList();
    }
}