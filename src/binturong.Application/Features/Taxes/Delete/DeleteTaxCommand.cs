using Application.Abstractions.Messaging;

namespace Application.Features.Taxes.Delete;

public sealed record DeleteTaxCommand(Guid TaxId) : ICommand;
