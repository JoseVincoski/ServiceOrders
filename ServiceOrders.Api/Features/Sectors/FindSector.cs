using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Domain.Sectors;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Sectors;

public class FindSectorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/sectors/{sectorId:guid}", async (
            [FromRoute] Guid sectorId,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<FindSector.Response> result = await FindSector.HandleAsync(sectorId, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.NotFound(error)
            );
        })
        .WithName("FindSector")
        .WithTags(EndpointTags.Sector)
        .WithOpenApi();
    }
}

public static class FindSector
{
    public sealed record Response(Guid Id, string Name);

    public static async Task<Result<Response>> HandleAsync(
        Guid sectorId,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Sector? sector = await dbContext.Sectors.FirstOrDefaultAsync(s => s.Id == sectorId, cancellationToken);
        if (sector is null)
        {
            return Result.Failure<Response>(Error.NotFound(
                "Sector.NotFound",
                $"Sector with ID {sectorId} was not found."));
        }

        var response = new Response(
            sector.Id,
            sector.Name);
        return Result.Success(response);
    }
}