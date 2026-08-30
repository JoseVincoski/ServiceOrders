using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain;
using ServiceOrders.Api.Domain.Items;
using ServiceOrders.Api.Domain.Items.PurchaseItem;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Items;

public class CreatePurchaseEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/items/purchases", async (
            [FromBody] CreatePurchase.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<CreatePurchase.Response> result = await CreatePurchase.HandleAsync(request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response), // Usando OK limpo como combinado no YAGNI
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("CreatePurchase")
        .WithSummary("Creates an item purchase")
        .WithTags(EndpointTags.Item)
        .WithValidation<CreatePurchase.Request>()
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Master.ToString()))
        .WithOpenApi();
    }
}

public static class CreatePurchase
{
    public sealed record Request(Guid ItemId, int Quantity, decimal UnitPrice);
    public sealed record Response(Guid PurchaseId, DateTime PurchasedAt);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.ItemId).NotEmpty();
            RuleFor(c => c.Quantity).GreaterThan(0);
            RuleFor(c => c.UnitPrice).GreaterThanOrEqualTo(0);
        }
    }

    public static async Task<Result<Response>> HandleAsync(
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        bool itemExists = await dbContext.Set<Item>().AnyAsync(i => i.Id == request.ItemId, cancellationToken);
        if (!itemExists)
        {
            return Result.Failure<Response>(Error.Problem("Item.NotFound", $"Item with ID {request.ItemId} was not found."));
        }

        Result<PurchaseItem> purchaseResult = PurchaseItem.Create(request.ItemId, request.Quantity, request.UnitPrice);
        if (purchaseResult.IsFailure)
        {
            return Result.Failure<Response>(purchaseResult.Error);
        }

        PurchaseItem purchase = purchaseResult.Value;

        dbContext.Set<PurchaseItem>().Add(purchase);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new Response(purchase.Id, purchase.PurchasedAtUtc));
    }
}