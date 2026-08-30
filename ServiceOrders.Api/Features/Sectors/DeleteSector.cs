using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain.Sectors;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Sectors;

public class DeleteSectorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/sectors/{id:guid}", async (
            [FromRoute] Guid id,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result result = await DeleteSector.HandleAsync(id, dbContext, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("DeleteSector")
        .WithSummary("Deletes a sector via Id")
        .WithTags("Sector")
        .WithOpenApi();
    }
}

public static class DeleteSector
{
    public static async Task<Result> HandleAsync(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Sector? sector = await dbContext.Sectors.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sector is null)
        {
            return Result.Failure(Error.NotFound("Sector.NotFound", $"Sector with ID {id} was not found."));
        }

        bool usedInEquipment = await dbContext.Equipments.AnyAsync(e => e.SectorId == id, cancellationToken);
        if (usedInEquipment)
        {
            return Result.Failure(Error.Conflict("Sector.InUse", $"Sector with ID {id} is in use on at least one equipment. It can not be deleted."));
        }

        dbContext.Sectors.Remove(sector);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}