namespace ExpenseManager.Models;

public class Filter {
    public DateTime? DateTimeFrom { get; set; }
    public DateTime? DateTimeTo { get; set; }

    public readonly Dictionary<int, bool> Categories;

    public Filter(Dictionary<int, string> allCategories) {
        Categories = new Dictionary<int, bool>();
        foreach (var (id, _) in allCategories) {
            Categories[id] = true;
        }

        DateTimeFrom = null;
        DateTimeTo = null;
    }

    public bool ExpenseBelongs(Expense expense) {
        DateTime expenseDatetime = DateTime.Parse(expense.DateTime);
        if (DateTimeFrom != null && expenseDatetime < DateTimeFrom) {
            return false;
        }

        if (DateTimeTo != null && expenseDatetime > DateTimeTo) {
            return false;
        }

        return Categories[expense.CategoryId];
    }

    public void UpdateFilterCategory(Dictionary<int, string> allCategories) {
        foreach (var (id, _) in allCategories) {
            Categories[id] = true;
        }
    }
    public void ResetFilter() {
        DateTimeTo = null;
        DateTimeFrom = null;
        foreach (var kvp in Categories) {
            Categories[kvp.Key] = true;
        }
    }
};