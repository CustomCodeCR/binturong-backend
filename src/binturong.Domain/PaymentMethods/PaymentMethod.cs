using SharedKernel;

namespace Domain.PaymentMethods;

public sealed class PaymentMethod : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<Domain.Payments.Payment> Payments { get; set; } =
        new List<Domain.Payments.Payment>();
}
