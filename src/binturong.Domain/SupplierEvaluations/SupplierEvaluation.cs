using SharedKernel;

namespace Domain.SupplierEvaluations;

public sealed class SupplierEvaluation : Entity
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }

    public int Score { get; set; } // 1..5
    public string Classification { get; set; } = string.Empty; // Reliable | LowPerformance

    public string Comment { get; set; } = string.Empty;
    public DateTime EvaluatedAtUtc { get; set; }

    public static Result<SupplierEvaluation> Create(
        Guid supplierId,
        int score,
        string comment,
        DateTime evaluatedAtUtc
    )
    {
        if (supplierId == Guid.Empty)
            return Result.Failure<SupplierEvaluation>(SupplierEvaluationErrors.SupplierRequired);

        if (score < 1 || score > 5)
            return Result.Failure<SupplierEvaluation>(SupplierEvaluationErrors.ScoreInvalid);

        if (string.IsNullOrWhiteSpace(comment))
            return Result.Failure<SupplierEvaluation>(SupplierEvaluationErrors.CommentRequired);

        var classification = score >= 4 ? "Reliable" : "LowPerformance";

        var e = new SupplierEvaluation
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            Score = score,
            Classification = classification,
            Comment = comment.Trim(),
            EvaluatedAtUtc = evaluatedAtUtc,
        };

        e.RaiseCreated();

        return Result.Success(e);
    }

    public void RaiseCreated() =>
        Raise(
            new SupplierEvaluationCreatedDomainEvent(
                Id,
                SupplierId,
                Score,
                Classification,
                Comment,
                EvaluatedAtUtc
            )
        );
}
