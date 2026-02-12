using Api.Security;
using Application.Abstractions.Messaging;
using Application.Features.Payroll.Calculate;
using Application.Features.Payroll.Commission.Adjust;
using Application.Features.Payroll.Create;
using Application.Features.Payroll.Delete;
using Application.Features.Payroll.GetPayrollById;
using Application.Features.Payroll.GetPayrolls;
using Application.Features.Payroll.History;
using Application.Features.Payroll.Overtime.Delete;
using Application.Features.Payroll.Overtime.Register;
using Application.Features.Payroll.Payslips;
using Application.Features.Payroll.Update;
using Application.ReadModels.Payroll;
using Application.Security.Scopes;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Payroll;

public sealed class PayrollEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payroll").WithTags("Payroll");

        group
            .MapGet(
                "/",
                async (
                    [FromQuery] string? periodCode,
                    [FromQuery] DateTime? fromUtc,
                    [FromQuery] DateTime? toUtc,
                    [FromQuery] string? status,
                    [FromQuery] int? page,
                    [FromQuery] int? pageSize,
                    IQueryHandler<GetPayrollsQuery, IReadOnlyList<PayrollReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetPayrollsQuery(
                            periodCode,
                            fromUtc,
                            toUtc,
                            status,
                            page ?? 1,
                            pageSize ?? 50
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PayrollRead);

        group
            .MapGet(
                "/{payrollId:guid}",
                async (
                    Guid payrollId,
                    IQueryHandler<GetPayrollByIdQuery, PayrollReadModel> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new GetPayrollByIdQuery(payrollId), ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PayrollRead);

        group
            .MapPost(
                "/",
                async (
                    CreatePayrollRequest req,
                    ICommandHandler<CreatePayrollCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new CreatePayrollCommand(
                        req.PeriodCode,
                        req.StartDate,
                        req.EndDate,
                        req.PayrollType
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/payroll/{result.Value}",
                            new { payrollId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PayrollCreate);

        group
            .MapPut(
                "/{payrollId:guid}",
                async (
                    Guid payrollId,
                    UpdatePayrollRequest req,
                    ICommandHandler<UpdatePayrollCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new UpdatePayrollCommand(
                            payrollId,
                            req.PeriodCode,
                            req.StartDate,
                            req.EndDate,
                            req.PayrollType,
                            req.Status
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.PayrollUpdate);

        group
            .MapDelete(
                "/{payrollId:guid}",
                async (
                    Guid payrollId,
                    ICommandHandler<DeletePayrollCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(new DeletePayrollCommand(payrollId), ct);
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.PayrollUpdate);

        group
            .MapPost(
                "/calculate",
                async (
                    CalculatePayrollRequest req,
                    ICommandHandler<CalculatePayrollCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var cmd = new CalculatePayrollCommand(
                        req.PeriodCode,
                        req.StartDate,
                        req.EndDate,
                        req.PayrollType,
                        req.AttendanceConfirmed
                    );

                    var result = await handler.Handle(cmd, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/payroll/{result.Value}",
                            new { payrollId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PayrollCreate);

        group
            .MapPost(
                "/overtime",
                async (
                    RegisterOvertimeRequest req,
                    ICommandHandler<RegisterPayrollOvertimeCommand, Guid> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new RegisterPayrollOvertimeCommand(
                            req.EmployeeId,
                            req.WorkDate,
                            req.Hours,
                            req.Notes
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Created(
                            $"/api/payroll/overtime/{result.Value}",
                            new { overtimeId = result.Value }
                        );
                }
            )
            .RequireScope(SecurityScopes.PayrollOvertimeManage);

        group
            .MapDelete(
                "/overtime/{overtimeId:guid}",
                async (
                    Guid overtimeId,
                    ICommandHandler<DeletePayrollOvertimeCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new DeletePayrollOvertimeCommand(overtimeId),
                        ct
                    );
                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.PayrollOvertimeManage);

        group
            .MapPut(
                "/{payrollId:guid}/details/{detailId:guid}/commission",
                async (
                    Guid payrollId,
                    Guid detailId,
                    AdjustCommissionRequest req,
                    ICommandHandler<AdjustPayrollCommissionCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new AdjustPayrollCommissionCommand(
                            payrollId,
                            detailId,
                            req.CommissionAmount
                        ),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.NoContent();
                }
            )
            .RequireScope(SecurityScopes.PayrollCommissionManage);

        group
            .MapGet(
                "/{payrollId:guid}/payslips/{employeeId:guid}.pdf",
                async (
                    Guid payrollId,
                    Guid employeeId,
                    IQueryHandler<GeneratePayslipPdfQuery, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GeneratePayslipPdfQuery(payrollId, employeeId),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/pdf",
                            $"payslip_{payrollId:N}_{employeeId:N}.pdf"
                        );
                }
            )
            .RequireScope(SecurityScopes.PayrollPayslipRead);

        group
            .MapPost(
                "/{payrollId:guid}/payslips/{employeeId:guid}/send-email",
                async (
                    Guid payrollId,
                    Guid employeeId,
                    ICommandHandler<SendPayslipEmailCommand> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new SendPayslipEmailCommand(payrollId, employeeId),
                        ct
                    );
                    return result.IsFailure ? Results.BadRequest(result.Error) : Results.Accepted();
                }
            )
            .RequireScope(SecurityScopes.PayrollPayslipSend);

        group
            .MapGet(
                "/employees/{employeeId:guid}/history",
                async (
                    Guid employeeId,
                    [FromQuery] DateTime? fromUtc,
                    [FromQuery] DateTime? toUtc,
                    IQueryHandler<
                        GetEmployeePaymentHistoryQuery,
                        IReadOnlyList<EmployeePaymentHistoryRow>
                    > handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new GetEmployeePaymentHistoryQuery(employeeId, fromUtc, toUtc),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.PayrollRead);

        group
            .MapGet(
                "/employees/{employeeId:guid}/history/export.pdf",
                async (
                    Guid employeeId,
                    [FromQuery] DateTime? fromUtc,
                    [FromQuery] DateTime? toUtc,
                    IQueryHandler<ExportEmployeePaymentHistoryPdfQuery, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportEmployeePaymentHistoryPdfQuery(employeeId, fromUtc, toUtc),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/pdf",
                            $"payments_{employeeId:N}.pdf"
                        );
                }
            )
            .RequireScope(SecurityScopes.PayrollExport);

        group
            .MapGet(
                "/employees/{employeeId:guid}/history/export.xlsx",
                async (
                    Guid employeeId,
                    [FromQuery] DateTime? fromUtc,
                    [FromQuery] DateTime? toUtc,
                    IQueryHandler<ExportEmployeePaymentHistoryExcelQuery, byte[]> handler,
                    CancellationToken ct
                ) =>
                {
                    var result = await handler.Handle(
                        new ExportEmployeePaymentHistoryExcelQuery(employeeId, fromUtc, toUtc),
                        ct
                    );

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.File(
                            result.Value,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"payments_{employeeId:N}.xlsx"
                        );
                }
            )
            .RequireScope(SecurityScopes.PayrollExport);
    }
}
