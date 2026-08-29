using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Domain.Items;

public sealed class Item
{
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 500;

    private Item() { }

    private Item(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public static Result<Item> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Item>(Error.Problem("Item.EmptyName", "Item name cannot be empty."));
        }

        return new Item(Guid.NewGuid(), name.Trim(), description.Trim());
    }
}