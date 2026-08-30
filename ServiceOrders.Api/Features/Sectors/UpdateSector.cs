using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Domain.Sectors;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Features.Sectors;

public class UpdateSectorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/sectors/{id:guid}", async (
            [FromRoute] Guid id,
            [FromBody] UpdateSector.Request request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            Result<UpdateSector.Response> result = await UpdateSector.HandleAsync(id, request, dbContext, cancellationToken);

            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error)
            );
        })
        .WithName("UpdateSector")
        .WithSummary("Updates sector information")
        .WithTags("Sector")
        .WithValidation<UpdateSector.Request>()
        .WithOpenApi();
    }
}

public static class UpdateSector
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
        Guid id,
        Request request,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        Sector? sector = await dbContext.Sectors.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sector is null)
        {
            return Result.Failure<Response>(Error.Problem("Sector.NotFound", $"Sector with ID {id} was not found."));
        }

        sector.UpdateName(request.Name);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new Response(
            sector.Id,
            sector.Name);

        return Result.Success(response);
    }
}