using SharedKernel;

namespace Domain.JournalEntryDetails;

public sealed class JournalEntryDetail : Entity
{
    public Guid Id { get; set; }
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? CostCenterId { get; set; }
    public string Description { get; set; } = string.Empty;

    public Domain.JournalEntries.JournalEntry? JournalEntry { get; set; }
    public Domain.AccountsChart.AccountChart? Account { get; set; }
    public Domain.CostCenters.CostCenter? CostCenter { get; set; }
}
