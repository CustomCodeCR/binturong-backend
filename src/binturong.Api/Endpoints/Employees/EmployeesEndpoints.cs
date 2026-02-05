using Application.Abstractions.Messaging;
using Application.Features.Employees.Attendance.CheckIn;
using Application.Features.Employees.Attendance.CheckOut;
using Application.Features.Employees.Create;
using Application.Features.Employees.Delete;
using Application.Features.Employees.GetEmployeeById;
using Application.Features.Employees.GetEmployees;
using Application.Features.Employees.Update;
using Application.ReadModels.Payroll;

namespace Api.Endpoints.Employees;

public sealed class EmployeesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees").WithTags("Employees");

        // GET list
        group.MapGet(
            "/",
            async (
                int? page,
                int? pageSize,
                string? search,
                IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeReadModel>> handler,
                CancellationToken ct
            ) =>
            {
                var query = new GetEmployeesQuery(page ?? 1, pageSize ?? 50, search);
                var result = await handler.Handle(query, ct);

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        // GET by id
        group.MapGet(
            "/{id:guid}",
            async (
                Guid id,
                IQueryHandler<GetEmployeeByIdQuery, EmployeeReadModel> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetEmployeeByIdQuery(id), ct);
                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            }
        );

        // CREATE
        group.MapPost(
            "/",
            async (
                CreateEmployeeRequest req,
                ICommandHandler<CreateEmployeeCommand, Guid> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new CreateEmployeeCommand(
                    UserId: req.UserId,
                    BranchId: req.BranchId,
                    FullName: req.FullName,
                    NationalId: req.NationalId,
                    JobTitle: req.JobTitle,
                    BaseSalary: req.BaseSalary,
                    HireDate: req.HireDate,
                    TerminationDate: req.TerminationDate,
                    IsActive: req.IsActive
                );

                var result = await handler.Handle(cmd, ct);

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Created(
                        $"/api/employees/{result.Value}",
                        new { employeeId = result.Value }
                    );
            }
        );

        // UPDATE
        group.MapPut(
            "/{id:guid}",
            async (
                Guid id,
                UpdateEmployeeRequest req,
                ICommandHandler<UpdateEmployeeCommand> handler,
                CancellationToken ct
            ) =>
            {
                var cmd = new UpdateEmployeeCommand(
                    EmployeeId: id,
                    UserId: req.UserId,
                    BranchId: req.BranchId,
                    FullName: req.FullName,
                    JobTitle: req.JobTitle,
                    BaseSalary: req.BaseSalary,
                    TerminationDate: req.TerminationDate,
                    IsActive: req.IsActive
                );

                var result = await handler.Handle(cmd, ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // DELETE
        group.MapDelete(
            "/{id:guid}",
            async (Guid id, ICommandHandler<DeleteEmployeeCommand> handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new DeleteEmployeeCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // CHECK-IN
        group.MapPost(
            "/{id:guid}/check-in",
            async (
                Guid id,
                ICommandHandler<EmployeeCheckInCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new EmployeeCheckInCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );

        // CHECK-OUT
        group.MapPost(
            "/{id:guid}/check-out",
            async (
                Guid id,
                ICommandHandler<EmployeeCheckOutCommand> handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new EmployeeCheckOutCommand(id), ct);
                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            }
        );
    }
}
