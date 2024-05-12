namespace ExpenseManager.Models;

public record Expense(int Id, int UserId, decimal Amount, int CategoryId, string? Description, string DateTime);