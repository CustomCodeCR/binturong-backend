using Application.Abstractions.Messaging;

namespace Application.Features.Suppliers.Attachments.Remove;

public sealed record RemoveSupplierAttachmentCommand(Guid SupplierId, Guid AttachmentId) : ICommand;
