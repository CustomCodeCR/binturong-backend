using SharedKernel;

namespace Domain.CostCenters;

public sealed class CostCenter : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<Domain.AccountsChart.AccountChart> Accounts { get; set; } =
        new List<Domain.AccountsChart.AccountChart>();
    public ICollection<Domain.JournalEntryDetails.JournalEntryDetail> JournalEntryDetails { get; set; } =
        new List<Domain.JournalEntryDetails.JournalEntryDetail>();
}
