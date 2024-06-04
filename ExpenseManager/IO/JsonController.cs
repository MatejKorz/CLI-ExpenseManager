using System.Runtime.Serialization;
using System.Text.Json;
using ExpenseManager.Data;
using ExpenseManager.Models;

namespace ExpenseManager.IO;

public record ExpenseDto(int Id, decimal Amount, string Category, string? Description, string DateTime);

public class JsonController {
    public async Task<string> Serialize(string filepath, List<Expense> expenses, Dictionary<int, string> categories) {
        try {
            List<ExpenseDto> exportExpense = new List<ExpenseDto>();
            foreach (var exp in expenses) {
                ExpenseDto expenseDto = new ExpenseDto(exp.Id, exp.Amount, categories[exp.CategoryId], exp.Description,
                    exp.DateTime);
                exportExpense.Add(expenseDto);
            }

            var options = new JsonSerializerOptions {
                WriteIndented = true,
            };

            string jsonString = JsonSerializer.Serialize(exportExpense, options);
            await File.WriteAllTextAsync(filepath, jsonString);
            return string.Empty;
        } catch (Exception e) {
            return Utils.MakeErrorMessage(e.Message);
        }
    }

    public async Task<string> Deserialize(int userId, string filepath, DatabaseController dbController) {
        try {
            var categories = await dbController.GetUserCategories(userId);
            string json = await File.ReadAllTextAsync(filepath);
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };
            List<ExpenseDto>? expensesDto = JsonSerializer.Deserialize<List<ExpenseDto>>(json, options);
            if (expensesDto == null) {
                return Utils.MakeErrorMessage("file is empty");
            }

            foreach (var expDto in expensesDto) {
                if (!categories.ContainsValue(expDto.Category)) {
                    await dbController.AddUserCategory(userId, expDto.Category);
                }

                var category = dbController.GetUserCategory(userId, expDto.Category);
                var exp = new Expense(0, userId, expDto.Amount, category.Result.Id, expDto.Description, expDto.DateTime);
                dbController.AddExpense(exp);
            }

            return string.Empty;
        }
        catch (Exception e) {
            return Utils.MakeErrorMessage(e.Message);
        }

    }
}