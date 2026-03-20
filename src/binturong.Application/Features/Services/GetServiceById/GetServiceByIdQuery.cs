using Application.Abstractions.Messaging;
using Application.ReadModels.Services;

namespace Application.Features.Services.GetServiceById;

public sealed record GetServiceByIdQuery(Guid ServiceId) : IQuery<ServiceReadModel>;
