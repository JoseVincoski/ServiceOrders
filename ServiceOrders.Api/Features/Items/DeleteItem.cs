using Carter;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain;
using ServiceOrders.Api.Domain.Items;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;
using System.Xml.Linq;

namespace ServiceOrders.Api.Features.Items;

public class DeleteItemEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/items/{id:guid}", async (
            Guid id,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result result = await DeleteItem.HandleAsync(id, dbContext, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("DeleteItem")
        .WithSummary("Deletes an item via Id")
        .WithTags(EndpointTags.Item)
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Master.ToString()))
        .WithOpenApi();
    }
}

public static class DeleteItem
{
    public static async Task<Result> HandleAsync(
        Guid id,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Item? item = await dbContext.Set<Item>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (item is null)
        {
            return Result.Failure(Error.Problem("Item.NotFound", $"Item with ID {id} was not found."));
        }

        dbContext.Set<Item>().Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}