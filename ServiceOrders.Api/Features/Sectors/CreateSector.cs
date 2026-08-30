using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Domain.Sectors;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Sectors;

public class CreateSectorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/sectors", async (
            [FromBody] CreateSector.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<CreateSector.Response> result = await CreateSector.HandleAsync(request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Created($"api/sectors/{response.Id}", response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("CreateSector")
        .WithSummary("Creates a new sector")
        .WithTags(EndpointTags.Sector)
        .WithValidation<CreateSector.Request>()
        .WithOpenApi();
    }
}

public static class CreateSector
{
    public sealed record Request(string Name);
    public sealed record Response(Guid Id, string Name);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(Sector.MaxNameLength);
        }
    }

    public static async Task<Result<Response>> HandleAsync(
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        bool sectorExists = await dbContext.Sectors.AnyAsync(s => s.Name == request.Name, cancellationToken);
        if (sectorExists)
        {
            return Result.Failure<Response>(Error.Problem("Sector.AlreadyExists", $"Sector with Name {request.Name} already exists."));
        }

        Result<Sector> sectorResult = Sector.Create(request.Name);
        if (sectorResult.IsFailure)
        {
            return Result.Failure<Response>(sectorResult.Error);
        }

        Sector sector = sectorResult.Value;
        dbContext.Sectors.Add(sector);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new Response(
            sector.Id,
            sector.Name);
        return Result.Success(response);
    }
}