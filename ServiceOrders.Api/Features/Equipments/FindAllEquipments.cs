using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Equipments;

public class FindAllEquipmentsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/equipments", async (
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<List<FindAllEquipments.Response>> result = await FindAllEquipments.HandleAsync(dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.NotFound(error)
            );
        })
        .WithName("FindAllEquipments")
        .WithSummary("Retrieves a list of all equipments")
        .WithTags(EndpointTags.Equipment)
        .WithOpenApi();
    }
}

public static class FindAllEquipments
{
    public sealed record Response(Guid Id, string Name, string Description, Guid SectorId);

    public static async Task<Result<List<Response>>> HandleAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<Response> response = await dbContext.Equipments
            .AsNoTracking()
            .Select(e => new Response(
                e.Id,
                e.Name,
                e.Description,
                e.SectorId))
            .ToListAsync(cancellationToken);

        return Result.Success(response);
    }
}