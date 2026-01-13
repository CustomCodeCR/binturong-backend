using SharedKernel;

namespace Domain.AccountingPeriods;

public sealed class AccountingPeriod : Entity
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public ICollection<Domain.JournalEntries.JournalEntry> JournalEntries { get; set; } =
        new List<Domain.JournalEntries.JournalEntry>();
}
