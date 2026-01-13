using SharedKernel;

namespace Domain.JournalEntries;

public sealed class JournalEntry : Entity
{
    public Guid Id { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public DateOnly EntryDate { get; set; }
    public Guid? PeriodId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SourceModule { get; set; } = string.Empty;
    public int? SourceId { get; set; }

    public Domain.AccountingPeriods.AccountingPeriod? Period { get; set; }
    public ICollection<Domain.JournalEntryDetails.JournalEntryDetail> Details { get; set; } =
        new List<Domain.JournalEntryDetails.JournalEntryDetail>();
}
