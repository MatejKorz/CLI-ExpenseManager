namespace ExpenseManager.Models;

public record Expense(int Id, int UserId, decimal Amount, Categories Category, string? Description, string DateTime);