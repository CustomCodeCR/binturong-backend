using Application.Abstractions.Messaging;
using Application.Features.Clients.Addresses.Add;
using Application.Features.Clients.Addresses.Remove;
using Application.Features.Clients.Addresses.SetPrimary;
using Application.Features.Clients.Addresses.Update;

namespace Api.Endpoints.Clients;

public sealed class ClientAddressesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients/{clientId:guid}/addresses")
            .WithTags("ClientAddresses");

        // =========================
        // ADD
        // POST /api/clients/{clientId}/addresses
        // =========================
        group.MapPost(
            "/",
            async (
                Guid clientId,
                AddClientAddressRequest req,
                ICommandHandler<AddClientAddressCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new AddClientAddressCommand(
                    clientId,
                    req.AddressType,
                    req.AddressLine,
                    req.Province,
                    req.Canton,
                    req.District,
                    req.Notes,
                    req.IsPrimary
                );

                var result = await handler.Handle(cmd, ct);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Created(
                    $"/api/clients/{clientId}/addresses/{result.Value}",
                    new { addressId = result.Value }
                );
            }
        );

        // =========================
        // UPDATE
        // PUT /api/clients/{clientId}/addresses/{addressId}
        // =========================
        group.MapPut(
            "/{addressId:guid}",
            async (
                Guid clientId,
                Guid addressId,
                UpdateClientAddressRequest req,
                ICommandHandler<UpdateClientAddressCommand> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UpdateClientAddressCommand(
                    clientId,
                    addressId,
                    req.AddressType,
                    req.AddressLine,
                    req.Province,
                    req.Canton,
                    req.District,
                    req.Notes,
                    req.IsPrimary
                );

                var result = await handler.Handle(cmd, ct);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // DELETE
        // DELETE /api/clients/{clientId}/addresses/{addressId}
        // =========================
        group.MapDelete(
            "/{addressId:guid}",
            async (
                Guid clientId,
                Guid addressId,
                ICommandHandler<RemoveClientAddressCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new RemoveClientAddressCommand(clientId, addressId),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // =========================
        // SET PRIMARY
        // PUT /api/clients/{clientId}/addresses/{addressId}/primary
        // =========================
        group.MapPut(
            "/{addressId:guid}/primary",
            async (
                Guid clientId,
                Guid addressId,
                ICommandHandler<SetPrimaryClientAddressCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(
                    new SetPrimaryClientAddressCommand(clientId, addressId),
                    ct
                );
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
