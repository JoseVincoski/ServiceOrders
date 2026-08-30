using ServiceOrders.Api.Domain.Sectors;
using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Domain.Equipments;

public sealed class Equipment
{
    public const int MaxNameLength = 150;
    public const int MaxDescriptionLength = 500;

    private Equipment() { }

    private Equipment(Guid id, string name, string description, Guid sectorId)
    {
        Id = id;
        Name = name;
        Description = description;
        SectorId = sectorId;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid SectorId { get; private set; }
    public Sector Sector { get; private set; } = null!;

    public static Result<Equipment> Create(string name, string description, Guid sectorId)
    {
        var validation = ValidateState(name, sectorId);
        if (validation.IsFailure)
        {
            return Result.Failure<Equipment>(validation.Error);
        }

        return new Equipment(
            Guid.NewGuid(),
            name.Trim(),
            description.Trim(),
            sectorId);
    }

    public Result UpdateDetails(string newName, string newDescription, Guid newSectorId)
    {
        var validation = ValidateState(newName, newSectorId);
        if (validation.IsFailure)
        {
            return Result.Failure<Equipment>(validation.Error);
        }

        Name = newName.Trim();
        Description = newDescription.Trim();
        SectorId = newSectorId;

        return Result.Success();
    }

    private static Result ValidateState(string name, Guid sectorId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Equipment>(Error.Validation("Equipment.InvalidName", "The equipment name cannot be empty."));
        }

        if (sectorId == Guid.Empty)
        {
            return Result.Failure<Equipment>(Error.Validation("Equipment.EmptySectorId", "The sector ID cannot be empty."));
        }

        return Result.Success();
    }
}