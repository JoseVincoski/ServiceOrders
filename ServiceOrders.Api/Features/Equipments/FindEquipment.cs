using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Domain.Equipments;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Equipments;

public class FindEquipmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/equipments/{equipmentId:guid}", async (
            [FromRoute] Guid equipmentId,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<FindEquipment.Response> result = await FindEquipment.HandleAsync(equipmentId, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.NotFound(error)
            );
        })
        .WithName("FindEquipment")
        .WithSummary("Retrieves one equipment by Id")
        .WithTags(EndpointTags.Equipment)
        .WithOpenApi();
    }
}

public static class FindEquipment
{
    public sealed record Response(Guid Id, string Name, string Description, Guid SectorId);

    public static async Task<Result<Response>> HandleAsync(
        Guid equipmentId,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Equipment? equipment = await dbContext.Equipments.FirstOrDefaultAsync(e => e.Id == equipmentId, cancellationToken);
        if (equipment is null)
        {
            return Result.Failure<Response>(Error.NotFound("Equipment.NotFound", $"Equipment with ID {equipmentId} was not found."));
        }

        var response = new Response(
            equipment.Id,
            equipment.Name,
            equipment.Description,
            equipment.SectorId);
        return Result.Success(response);
    }
}