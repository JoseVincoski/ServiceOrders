namespace ServiceOrders.Api.Domain.ServiceOrders.ServiceOrderItems;

public sealed class ServiceOrderItem
{
    private ServiceOrderItem() { }

    internal ServiceOrderItem(Guid id, Guid serviceOrderId, Guid itemId, int quantity)
    {
        Id = id;
        ServiceOrderId = serviceOrderId;
        ItemId = itemId;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid ItemId { get; private set; }
    public int Quantity { get; private set; }

    internal void IncreaseQuantity(int amount)
    {
        Quantity += amount;
    }
}