using Application.Abstractions.Messaging;

namespace Application.Features.Clients.Attachments.Remove;

public sealed record RemoveClientAttachmentCommand(Guid ClientId, Guid AttachmentId) : ICommand;
