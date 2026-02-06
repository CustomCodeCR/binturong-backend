using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Clients.Contacts.Add;
using Application.Features.Clients.Contacts.Remove;
using Application.Features.Clients.Contacts.SetPrimary;
using Application.Features.Clients.Contacts.Update;
using Application.Security.Scopes;

namespace Api.Endpoints.Clients;

public sealed class ClientContactsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients/{clientId:guid}/contacts")
            .WithTags("ClientContacts");

        group
            .MapPost(
                "/",
                async (
                    Guid clientId,
                    AddClientContactRequest req,
                    ICommandHandler<AddClientContactCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new AddClientContactCommand(
                        clientId,
                        req.Name,
                        req.JobTitle,
                        req.Email,
                        req.Phone,
                        req.IsPrimary
                    );
                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/clients/{clientId}/contacts/{result.Value}",
                            new { contactId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.ClientsUpdate);

        group
            .MapPut(
                "/{contactId:guid}",
                async (
                    Guid clientId,
                    Guid contactId,
                    UpdateClientContactRequest req,
                    ICommandHandler<UpdateClientContactCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new UpdateClientContactCommand(
                        clientId,
                        contactId,
                        req.Name,
                        req.JobTitle,
                        req.Email,
                        req.Phone,
                        req.IsPrimary
                    );
                    var result = await handler.Handle(cmd, ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ClientsUpdate);

        group
            .MapDelete(
                "/{contactId:guid}",
                async (
                    Guid clientId,
                    Guid contactId,
                    ICommandHandler<RemoveClientContactCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RemoveClientContactCommand(clientId, contactId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ClientsUpdate);

        group
            .MapPut(
                "/{contactId:guid}/primary",
                async (
                    Guid clientId,
                    Guid contactId,
                    ICommandHandler<SetPrimaryClientContactCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new SetPrimaryClientContactCommand(clientId, contactId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ClientsUpdate);
    }
}
