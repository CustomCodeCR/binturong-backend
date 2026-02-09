using SharedKernel;

namespace Domain.SupplierEvaluations;

public static class SupplierEvaluationErrors
{
    public static readonly Error SupplierRequired = Error.Validation(
        "SupplierEvaluations.SupplierRequired",
        "SupplierId is required."
    );

    public static readonly Error ScoreInvalid = Error.Validation(
        "SupplierEvaluations.ScoreInvalid",
        "Score must be between 1 and 5."
    );

    public static readonly Error CommentRequired = Error.Validation(
        "SupplierEvaluations.CommentRequired",
        "Comment is required."
    );

    public static Error NotFound(Guid id) =>
        Error.NotFound("SupplierEvaluations.NotFound", $"Supplier evaluation '{id}' not found.");
}
