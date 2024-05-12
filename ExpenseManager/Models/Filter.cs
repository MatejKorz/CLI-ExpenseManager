namespace ExpenseManager.Models;

public class Filter {
    public DateTime DateTimeFrom;
    public DateTime DateTimeTo;

    public Dictionary<string, bool> categories;

    public bool Type;

    
};