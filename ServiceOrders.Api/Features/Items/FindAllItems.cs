using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain.Items;
using ServiceOrders.Api.Domain.Items.PurchaseItem;
using ServiceOrders.Api.Domain.ServiceOrders.ServiceOrderItems;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Items;

public class FindAllItemsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/items", async (
            [FromQuery] string? searchTerm,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<List<FindAllItems.Response>> result = await FindAllItems.HandleAsync(searchTerm, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("FindAllItems")
        .WithSummary("Retrieves a list of all items based on search term")
        .WithTags(EndpointTags.Item)
        .RequireAuthorization()
        .WithOpenApi();
    }
}

public static class FindAllItems
{
    public sealed record Response(Guid Id, string Name, string Description, int CurrentStock);

    public static async Task<Result<List<Response>>> HandleAsync(
        string? searchTerm,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        IQueryable<Item> query = dbContext.Set<Item>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = $"%{searchTerm.ToLowerInvariant()}%";
            query = query.Where(i => EF.Functions.ILike(i.Name, term) || EF.Functions.ILike(i.Description, term));
        }

        List<Response> items = await query
            .OrderBy(i => i.Name)
            .Select(i => new Response(
                i.Id,
                i.Name,
                i.Description,
                dbContext.Set<PurchaseItem>().Where(p => p.ItemId == i.Id).Sum(p => (int?)p.Quantity) ?? 0
                - dbContext.Set<ServiceOrderItem>().Where(so => so.ItemId == i.Id).Sum(so => (int?)so.Quantity) ?? 0
            ))
            .ToListAsync(cancellationToken);

        var response = new List<Response>(items);

        return Result.Success(response);
    }
}