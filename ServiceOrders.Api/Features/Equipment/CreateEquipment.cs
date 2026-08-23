using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ServiceOrder.Api.Extensions;
using ServiceOrder.Api.Shared;
using static ServiceOrders.Api.Features.Equipment.CreateEquipment;

namespace ServiceOrders.Api.Features.Equipment
{
    public class CreateEquipmentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/equipments", async ([FromBody] Request request) =>
            {
                return await HandleAsync(request);
            })
            .WithName("CreateEquipment")
            .WithTags(EndpointTags.Equipment)
            .WithValidation<Request>()
            .WithOpenApi();
        }
    }

    public static class CreateEquipment
    {
        public sealed record Request(string Name, string Description);
        public sealed record Response(Guid Id, string Name, string Description);

        public class CreateEquipmentValidator : AbstractValidator<Request>
        {
            public CreateEquipmentValidator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(1000);
                RuleFor(x => x.Description).NotEmpty().MaximumLength(10000);
            }
        }

        public static async Task<Response> HandleAsync(Request request)
        {
            //var equipment = new Equipment
            //{
            //    Name = request.Name,
            //    Description = request.Description
            //};
            //dbContext.Equipments.Add(equipment);
            //await dbContext.SaveChangesAsync();

            //return new Response(equipment.Id, equipment.Name, equipment.Description);

            return new Response(Guid.NewGuid(), request.Name, request.Description);
        }
    }
}