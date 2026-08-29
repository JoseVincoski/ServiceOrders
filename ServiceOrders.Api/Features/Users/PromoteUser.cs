using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain;
using ServiceOrders.Api.Domain.Users;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Users;

public class PromoteUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/users/{id:guid}/role", async (
            Guid id,
            [FromBody] PromoteUser.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result result = await PromoteUser.HandleAsync(id, request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("PromoteUser")
        .WithTags(EndpointTags.User)
        .WithValidation<PromoteUser.Request>()
        .RequireAuthorization(policy => policy.RequireRole(UserRole.Master.ToString()))
        .WithOpenApi();
    }
}

public static class PromoteUser
{
    public sealed record Request(UserRole NewRole);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.NewRole).IsInEnum();
        }
    }

    public static async Task<Result> HandleAsync(
        Guid id,
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        User? user = await dbContext.Set<User>().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.Problem("User.NotFound", $"User with ID {id} was not found."));
        }

        Result promotionResult = user.PromoteTo(request.NewRole);
        if (promotionResult.IsFailure)
        {
            return promotionResult;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}