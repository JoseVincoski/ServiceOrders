using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain.Users;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ServiceOrders.Api.Features.Users;

public class LoginUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/login", async (
            [FromBody] LoginUser.Request request,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            Result<LoginUser.Response> result = await LoginUser.HandleAsync(request, dbContext, configuration, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("LoginUser")
        .WithSummary("Gets bearer token based on email and password")
        .WithTags(EndpointTags.User)
        .WithValidation<LoginUser.Request>()
        .WithOpenApi();
    }
}

public static class LoginUser
{
    public sealed record Request(string Email, string Password);
    public sealed record Response(string Token);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Email).NotEmpty().EmailAddress();
            RuleFor(c => c.Password).NotEmpty();
        }
    }

    public static async Task<Result<Response>> HandleAsync(
        Request request,
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        User? user = await dbContext
            .Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(Error.Problem("Login.Failed", "Invalid email or password."));
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Result.Failure<Response>(Error.Problem("Login.Failed", "Invalid email or password."));
        }

        string token = GenerateJwtToken(user, configuration);

        return Result.Success(new Response(token));
    }

    private static string GenerateJwtToken(User user, IConfiguration configuration)
    {
        string secretKey = configuration["Jwt:SecretKey"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Jwt");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
}