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
        var validation = ValidateState(name);
        if (validation.IsFailure)
        {
            return Result.Failure<Item>(validation.Error);
        }

        return new Item(Guid.NewGuid(), name.Trim(), description?.Trim() ?? string.Empty);
    }

    public Result UpdateDetails(string newName, string newDescription)
    {
        var validation = ValidateState(newName);
        if (validation.IsFailure)
        {
            return Result.Failure<Item>(validation.Error);
        }

        Name = newName.Trim();
        Description = newDescription?.Trim() ?? string.Empty;

        return Result.Success();
    }

    private static Result ValidateState(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("Item.EmptyName", "Item name cannot be empty."));
        }

        return Result.Success();
    }
}