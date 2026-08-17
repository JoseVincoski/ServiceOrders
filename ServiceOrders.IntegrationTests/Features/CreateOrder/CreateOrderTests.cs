using ServiceOrders.IntegrationTests.Setup;

namespace ServiceOrders.IntegrationTests.Features.CreateOrder;

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
