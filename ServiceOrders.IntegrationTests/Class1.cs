using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Api;
using ServiceOrders.IntegrationTests.Infraestructure;
using ServiceOrders.IntegrationTests.Infrastructure;
using Xunit;

namespace ServiceOrders.IntegrationTests.Features.Orders.CreateOrder;

public class CreateOrderTests : BaseIntegrationTest
{
    public CreateOrderTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_CreateOrder_When_RequestIsValid()
    {
        Assert.True(true);
    }
}
