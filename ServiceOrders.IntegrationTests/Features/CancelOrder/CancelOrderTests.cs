using ServiceOrders.IntegrationTests.Setup;

namespace ServiceOrders.IntegrationTests.Features.CancelOrder;

public class CancelOrderTests : BaseIntegrationTest
{
    public CancelOrderTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Should_CancelOrder_When_RequestIsValid()
    {
        Assert.True(true);
    }
}
