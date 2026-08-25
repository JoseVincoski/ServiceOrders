using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain.Equipments;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Equipments;

public class UpdateEquipmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/equipments/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateEquipment.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<UpdateEquipment.Response> result = await UpdateEquipment.HandleAsync(id, request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("UpdateEquipment")
        .WithTags("Equipment")
        .WithValidation<UpdateEquipment.Request>()
        .WithOpenApi();
    }
}

public static class UpdateEquipment
{
    public sealed record Request(string Name, string Description, Guid SectorId);
    public sealed record Response(Guid Id, string Name, string Description, Guid SectorId);
    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(150);
            RuleFor(c => c.Description).NotEmpty().MaximumLength(500);
            RuleFor(c => c.SectorId).NotEmpty();
        }
    }

    public static async Task<Result<Response>> HandleAsync(
        Guid id,
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Equipment? equipment = await dbContext.Equipments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (equipment is null)
        {
            return Result.Failure<Response>(Error.NotFound(
                "Equipment.NotFound",
                $"Equipment with ID {id} was not found."));
        }

        if (equipment.SectorId != request.SectorId)
        {
            bool sectorExists = await dbContext.Sectors.AnyAsync(s => s.Id == request.SectorId, cancellationToken);
            if (!sectorExists)
            {
                return Result.Failure<Response>(Error.NotFound(
                    "Sector.NotFound",
                    $"Sector with ID {request.SectorId} was not found."));
            }
        }

        Result updateResult = equipment.UpdateDetails(request.Name, request.Description, request.SectorId);
        if (updateResult.IsFailure)
        {
            return Result.Failure<Response>(updateResult.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new Response(
            equipment.Id,
            equipment.Name,
            equipment.Description,
            equipment.SectorId);

        return Result.Success(response);
    }
}