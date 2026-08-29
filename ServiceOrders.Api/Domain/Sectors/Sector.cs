using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Domain.Sectors;

public sealed class Sector
{
    public const int MaxNameLength = 100;

    private Sector() { }

    private Sector(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public static Result<Sector> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Sector>(Error.Problem(
                "Sector.InvalidName",
                "The sector name cannot be empty."));
        }

        return new Sector(Guid.NewGuid(), name.Trim());
    }

    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure(Error.Problem(
                "Sector.InvalidName",
                "The sector name cannot be empty."));
        }

        Name = newName.Trim();
        return Result.Success();
    }
}