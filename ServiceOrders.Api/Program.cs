using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ServiceOrder.Api.Extensions;
using ServiceOrders.Api.Database;
using ServiceOrders.Api.Extensions;
using ServiceOrders.Api.Shared.Exceptions;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
});

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.AddCarter();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddInfrastructureHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();
app.UseExceptionHandler();

app.MapInfrastructureHealthChecks();

app.UseHttpsRedirection();
app.Run();

public interface IApiMarker { }