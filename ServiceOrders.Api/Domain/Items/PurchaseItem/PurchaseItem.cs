using ServiceOrders.Api.Domain.Items;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Domain.Items.PurchaseItem;

public sealed class PurchaseItem
{
    private PurchaseItem() { }

    private PurchaseItem(Guid id, Guid itemId, int quantity, decimal unitPrice, DateTime purchasedAtUtc)
    {
        Id = id;
        ItemId = itemId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        PurchasedAtUtc = purchasedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTime PurchasedAtUtc { get; private set; }

    public Item Item { get; private set; } = null!;

    public static Result<PurchaseItem> Create(Guid itemId, int quantity, decimal unitPrice)
    {
        var validation = ValidateState(itemId, quantity, unitPrice);
        if (validation.IsFailure)
        {
            return Result.Failure<PurchaseItem>(validation.Error);
        }

        return new PurchaseItem(Guid.NewGuid(), itemId, quantity, unitPrice, DateTime.UtcNow);
    }

    private static Result ValidateState(Guid itemId, int quantity, decimal unitPrice)
    {
        if (itemId == Guid.Empty)
        {
            return Result.Failure<PurchaseItem>(Error.Validation("Purchase.EmptyItem", "Item ID is required."));
        }

        if (quantity <= 0)
        {
            return Result.Failure<PurchaseItem>(Error.Validation("Purchase.InvalidQuantity", "Quantity must be greater than zero."));
        }

        if (unitPrice < 0)
        {
            return Result.Failure<PurchaseItem>(Error.Validation("Purchase.InvalidPrice", "Price cannot be negative."));
        }

        return Result.Success();
    }
}