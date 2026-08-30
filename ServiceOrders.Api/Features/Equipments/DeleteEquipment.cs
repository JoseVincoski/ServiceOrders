using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain.Equipments;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Equipments;

public class DeleteEquipmentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/equipments/{id:guid}", async (
            [FromRoute] Guid id,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result result = await DeleteEquipment.HandleAsync(id, dbContext, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("DeleteEquipment")
        .WithSummary("Deletes an equipment via Id")
        .WithTags(EndpointTags.Equipment)
        .WithOpenApi();
    }
}

public static class DeleteEquipment
{
    public static async Task<Result> HandleAsync(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Equipment? equipment = await dbContext.Equipments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (equipment is null)
        {
            return Result.Failure(Error.NotFound("Equipment.NotFound", $"Equipment with ID {id} was not found."));
        }//TODO: Add check so I can't delete it if it is ever used in any OS or anything like this.

        dbContext.Equipments.Remove(equipment);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}