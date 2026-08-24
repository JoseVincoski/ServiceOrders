using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrder.Api.Database;
using ServiceOrder.Api.Extensions;
using ServiceOrder.Api.Shared;
using ServiceOrders.Api.Domain.Equipments;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Equipments;

public class CreateEquipmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/equipments", async (
            [FromBody] CreateEquipment.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<CreateEquipment.Response> result = await CreateEquipment.HandleAsync(request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Created($"api/equipments/{response.Id}", response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("CreateEquipment")
        .WithTags(EndpointTags.Equipment)
        .WithValidation<CreateEquipment.Request>()
        .WithOpenApi();
    }
}

public static class CreateEquipment
{
    public sealed record Request(string Name, string Description, Guid SectorId);
    public sealed record Response(Guid Id, string Name, string Description, Guid SectorId);

    public class CreateEquipmentValidator : AbstractValidator<Request>
    {
        public CreateEquipmentValidator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(150);
            RuleFor(c => c.Description).NotEmpty().MaximumLength(500);
            RuleFor(c => c.SectorId).NotEmpty();
        }
    }

    public static async Task<Result<Response>> HandleAsync(
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        bool sectorExists = await dbContext.Sectors.AnyAsync(s => s.Id == request.SectorId, cancellationToken);
        if (!sectorExists)
        {
            return Result.Failure<Response>(Error.Problem(
                "Sector.NotFound",
                $"Sector with ID {request.SectorId} was not found."));
        }

        Result<Equipment> equipmentResult = Equipment.Create(
            request.Name,
            request.Description,
            request.SectorId);
        if (equipmentResult.IsFailure)
        {
            return Result.Failure<Response>(equipmentResult.Error);
        }

        Equipment equipment = equipmentResult.Value;
        dbContext.Equipments.Add(equipment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new Response(
            equipment.Id,
            equipment.Name,
            equipment.Description,
            equipment.SectorId);
        return Result.Success(response);
    }
}