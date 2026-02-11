using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Contracts.Attachments.Delete;
using Application.Features.Contracts.Attachments.Upload;
using Application.Features.Contracts.Create;
using Application.Features.Contracts.Delete;
using Application.Features.Contracts.GetContractById;
using Application.Features.Contracts.GetContracts;
using Application.Features.Contracts.Milestones.Add;
using Application.Features.Contracts.Milestones.Remove;
using Application.Features.Contracts.Milestones.Update;
using Application.Features.Contracts.Update;
using Application.ReadModels.Sales;
using Application.Security.Scopes;

namespace Api.Endpoints.Contracts;

public sealed class ContractsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contracts").WithTags("Contracts");

        // =========================
        // GET /api/contracts
        // =========================
        group
            .MapGet(
                "/",
                async (
                    int? page,
                    int? pageSize,
                    string? search,
                    IQueryHandler<GetContractsQuery, IReadOnlyList<ContractReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetContractsQuery(page ?? 1, pageSize ?? 50, search),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ContractsRead);

        // =========================
        // GET /api/contracts/{id}
        // =========================
        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryHandler<GetContractByIdQuery, ContractReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetContractByIdQuery(id), ct);
                    return result.IsFailure
                        ? Results.NotFound(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.ContractsRead);

        // =========================
        // POST /api/contracts
        // =========================
        group
            .MapPost(
                "/",
                async (
                    CreateContractRequest req,
                    ICommandHandler<CreateContractCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new CreateContractCommand(
                        req.Code,
                        req.ClientId,
                        req.QuoteId,
                        req.SalesOrderId,
                        req.StartDate,
                        req.EndDate,
                        req.Status,
                        req.Description,
                        req.Notes,
                        req.Milestones.Select(m => new CreateContractMilestone(
                                m.Description,
                                m.Percentage,
                                m.Amount,
                                m.ScheduledDate
                            ))
                            .ToList()
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/contracts/{result.Value}",
                            new { contractId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.ContractsCreate);

        // =========================
        // PUT /api/contracts/{id}
        // =========================
        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateContractRequest req,
                    ICommandHandler<UpdateContractCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateContractCommand(
                            id,
                            req.Code,
                            req.ClientId,
                            req.QuoteId,
                            req.SalesOrderId,
                            req.StartDate,
                            req.EndDate,
                            req.Status,
                            req.Description,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ContractsUpdate);

        // =========================
        // DELETE /api/contracts/{id}
        // =========================
        group
            .MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandHandler<DeleteContractCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeleteContractCommand(id), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ContractsDelete);

        // ============================================================
        // Milestones
        // ============================================================

        // POST /api/contracts/{id}/milestones
        group
            .MapPost(
                "/{id:guid}/milestones",
                async (
                    Guid id,
                    AddContractMilestoneRequest req,
                    ICommandHandler<AddContractMilestoneCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new AddContractMilestoneCommand(
                            id,
                            req.Description,
                            req.Percentage,
                            req.Amount,
                            req.ScheduledDate
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/contracts/{id}/milestones/{result.Value}",
                            new { milestoneId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.ContractsMilestonesManage);

        // PUT /api/contracts/{id}/milestones/{milestoneId}
        group
            .MapPut(
                "/{id:guid}/milestones/{milestoneId:guid}",
                async (
                    Guid id,
                    Guid milestoneId,
                    UpdateContractMilestoneRequest req,
                    ICommandHandler<UpdateContractMilestoneCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdateContractMilestoneCommand(
                            id,
                            milestoneId,
                            req.Description,
                            req.Percentage,
                            req.Amount,
                            req.ScheduledDate,
                            req.IsBilled,
                            req.InvoiceId
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ContractsMilestonesManage);

        // DELETE /api/contracts/{id}/milestones/{milestoneId}
        group
            .MapDelete(
                "/{id:guid}/milestones/{milestoneId:guid}",
                async (
                    Guid id,
                    Guid milestoneId,
                    ICommandHandler<RemoveContractMilestoneCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RemoveContractMilestoneCommand(id, milestoneId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ContractsMilestonesManage);

        // ============================================================
        // Attachments (S3) - upload/delete
        // ============================================================

        // POST /api/contracts/{id}/attachments
        // multipart/form-data field name: "file"
        group
            .MapPost(
                "/{id:guid}/attachments",
                async (
                    Guid id,
                    IFormFile file,
                    ICommandHandler<UploadContractAttachmentCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    if (file is null || file.Length == 0)
                        return Results.BadRequest(
                            SharedKernel.Error.Validation(
                                "Contracts.Attachments.Missing",
                                "No file was provided."
                            )
                        );

                    await using var stream = file.OpenReadStream();

                    var result = await handler.Handle(
                        new UploadContractAttachmentCommand(
                            id,
                            file.FileName,
                            file.ContentType ?? "application/octet-stream",
                            file.Length,
                            stream
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/contracts/{id}/attachments/{result.Value}",
                            new { attachmentId = result.Value }
                        );
                }
            )
            .DisableAntiforgery()
            .RequireScope(SecurityScopes.ContractsAttachmentsUpload);

        // DELETE /api/contracts/{id}/attachments/{attachmentId}
        group
            .MapDelete(
                "/{id:guid}/attachments/{attachmentId:guid}",
                async (
                    Guid id,
                    Guid attachmentId,
                    ICommandHandler<DeleteContractAttachmentCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new DeleteContractAttachmentCommand(id, attachmentId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.ContractsAttachmentsDelete);
    }
}
