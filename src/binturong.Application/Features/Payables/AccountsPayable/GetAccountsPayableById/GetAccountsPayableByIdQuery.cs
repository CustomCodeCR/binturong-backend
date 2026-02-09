using Application.Abstractions.Messaging;
using Application.ReadModels.Payables;

namespace Application.Features.Payables.AccountsPayable.GetAccountsPayableById;

public sealed record GetAccountsPayableByIdQuery(Guid AccountPayableId)
    : IQuery<AccountsPayableReadModel>;
