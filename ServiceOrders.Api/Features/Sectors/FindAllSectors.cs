using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Shared;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Sectors;

public class FindAllSectorsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/sectors", async (
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<List<FindAllSectors.Response>> result = await FindAllSectors.HandleAsync(dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.NotFound(error)
            );
        })
        .WithName("FindAllSectors")
        .WithTags(EndpointTags.Sector)
        .WithOpenApi();
    }
}

public static class FindAllSectors
{
    public sealed record Response(Guid Id, string Name);

    public static async Task<Result<List<Response>>> HandleAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<Response> response = await dbContext.Sectors
            .AsNoTracking()
            .Select(s => new Response(
                s.Id,
                s.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(response);
    }
}