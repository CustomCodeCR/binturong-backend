using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Suppliers.Contacts.Add;
using Application.Features.Suppliers.Contacts.Remove;
using Application.Features.Suppliers.Contacts.SetPrimary;
using Application.Features.Suppliers.Contacts.Update;
using Application.Security.Scopes;

namespace Api.Endpoints.Suppliers;

public sealed class SupplierContactsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers/{supplierId:guid}/contacts")
            .WithTags("SupplierContacts");

        group
            .MapPost(
                "/",
                async (
                    Guid supplierId,
                    AddSupplierContactRequest req,
                    ICommandHandler<AddSupplierContactCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new AddSupplierContactCommand(
                        supplierId,
                        req.Name,
                        req.JobTitle,
                        req.Email,
                        req.Phone,
                        req.IsPrimary
                    );

                    var result = await handler.Handle(cmd, ct);

                    if (result.IsFailure)
                        return Results.BadRequest(result.Error);

                    return Results.Created(
                        $"/api/suppliers/{supplierId}/contacts/{result.Value}",
                        new { contactId = result.Value }
                    );
                }
            )
            .RequireScope(SecurityScopes.SuppliersUpdate);

        group
            .MapPut(
                "/{contactId:guid}",
                async (
                    Guid supplierId,
                    Guid contactId,
                    UpdateSupplierContactRequest req,
                    ICommandHandler<UpdateSupplierContactCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new UpdateSupplierContactCommand(
                        supplierId,
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
            .RequireScope(SecurityScopes.SuppliersUpdate);

        group
            .MapDelete(
                "/{contactId:guid}",
                async (
                    Guid supplierId,
                    Guid contactId,
                    ICommandHandler<RemoveSupplierContactCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RemoveSupplierContactCommand(supplierId, contactId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SuppliersUpdate);

        group
            .MapPut(
                "/{contactId:guid}/primary",
                async (
                    Guid supplierId,
                    Guid contactId,
                    ICommandHandler<SetPrimarySupplierContactCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new SetPrimarySupplierContactCommand(supplierId, contactId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.SuppliersUpdate);
    }
}
