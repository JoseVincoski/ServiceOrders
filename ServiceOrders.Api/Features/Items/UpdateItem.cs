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

public class UpdateItemEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/items/{id:guid}", async (
            Guid id,
            [FromBody] UpdateItem.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result result = await UpdateItem.HandleAsync(id, request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("UpdateItem")
        .WithSummary("Updates item information")
        .WithTags(EndpointTags.Item)
        .WithValidation<UpdateItem.Request>()
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Master.ToString()))
        .WithOpenApi();
    }
}

public static class UpdateItem
{
    public sealed record Request(string Name, string Description);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(150);
            RuleFor(c => c.Description).MaximumLength(500);
        }
    }

    public static async Task<Result> HandleAsync(
        Guid id,
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Item? item = await dbContext.Set<Item>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (item is null)
        {
            return Result.Failure(Error.Problem("Item.NotFound", $"Item with ID {id} was not found."));
        }

        if (item.Name != request.Name.Trim())
        {
            bool nameExists = await dbContext.Set<Item>().AnyAsync(i => i.Name == request.Name.Trim(), cancellationToken);
            if (nameExists)
            {
                return Result.Failure(Error.Problem("Item.AlreadyExists", "An item with this name already exists."));
            }
        }

        Result updateResult = item.UpdateDetails(request.Name, request.Description);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}