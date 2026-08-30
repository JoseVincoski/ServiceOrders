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

public class RegisterUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/register", async (
            [FromBody] RegisterUser.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<RegisterUser.Response> result = await RegisterUser.HandleAsync(request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Created($"api/users/{response.Id}", response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("RegisterUser")
        .WithSummary("Registers a new user as a Requester")
        .WithTags(EndpointTags.User)
        .WithValidation<RegisterUser.Request>()
        .WithOpenApi();
    }
}

public static class RegisterUser
{
    public sealed record Request(string Email, string Name, string Password);
    public sealed record Response(Guid Id, string Email);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(User.MaxEmailLength);
            RuleFor(c => c.Name).NotEmpty().MaximumLength(User.MaxNameLength);
            RuleFor(c => c.Password).NotEmpty().MinimumLength(User.MinPasswordLength);
        }
    }

    public static async Task<Result<Response>> HandleAsync(
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        bool emailExists = await dbContext.Set<User>()
            .AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Response>(Error.Problem("User.EmailAlreadyExists", "This email address is already in use."));
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        Result<User> userResult = User.Create(
            request.Name,
            request.Email,
            passwordHash,
            UserRole.Requester);

        if (userResult.IsFailure)
        {
            return Result.Failure<Response>(userResult.Error);
        }

        User user = userResult.Value;

        dbContext.Set<User>().Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new Response(user.Id, user.Email));
    }
}