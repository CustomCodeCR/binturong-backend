using SharedKernel;

namespace Domain.AccountsChart;

public sealed class AccountChart : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public Guid? ParentAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string Status { get; set; } = string.Empty;

    public Domain.AccountsChart.AccountChart? ParentAccount { get; set; }
    public ICollection<Domain.AccountsChart.AccountChart> Children { get; set; } =
        new List<Domain.AccountsChart.AccountChart>();

    public Domain.CostCenters.CostCenter? CostCenter { get; set; }
    public ICollection<Domain.JournalEntryDetails.JournalEntryDetail> JournalEntryDetails { get; set; } =
        new List<Domain.JournalEntryDetails.JournalEntryDetail>();
}
