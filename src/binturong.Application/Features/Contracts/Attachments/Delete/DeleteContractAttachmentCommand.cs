using Application.Abstractions.Messaging;

namespace Application.Features.Contracts.Attachments.Delete;

public sealed record DeleteContractAttachmentCommand(Guid ContractId, Guid AttachmentId) : ICommand;
