using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain;
using ServiceOrders.Api.Domain.Items;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Items;

public class CreateItemEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/items", async (
            [FromBody] CreateItem.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<CreateItem.Response> result = await CreateItem.HandleAsync(request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Created($"api/items/{response.Id}", response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("CreateItem")
        .WithSummary("Creates a new item")
        .WithTags(EndpointTags.Item)
        .WithValidation<CreateItem.Request>()
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Master.ToString()))
        .WithOpenApi();
    }
}

public static class CreateItem
{
    public sealed record Request(string Name, string Description);
    public sealed record Response(Guid Id, string Name);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(Item.MaxNameLength);
            RuleFor(c => c.Description).MaximumLength(Item.MaxDescriptionLength);
        }
    }

    public static async Task<Result<Response>> HandleAsync(
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        bool itemExists = await dbContext.Set<Item>().AnyAsync(i => i.Name == request.Name.Trim(), cancellationToken);
        if (itemExists)
        {
            return Result.Failure<Response>(Error.Problem("Item.AlreadyExists", $"An item with the name '{request.Name}' already exists."));
        }

        Result<Item> itemResult = Item.Create(request.Name, request.Description);
        if (itemResult.IsFailure)
        {
            return Result.Failure<Response>(itemResult.Error);
        }

        Item item = itemResult.Value;

        dbContext.Set<Item>().Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new Response(item.Id, item.Name));
    }
}