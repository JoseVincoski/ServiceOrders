using ServiceOrders.Api.Shared.Results;

namespace ServiceOrders.Api.Domain.Users;

public sealed class User
{
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 200;
    public const int MaxPasswordHashLength = 500;
    public const int MinPasswordLength = 6;

    private User() { }

    private User(Guid id, string name, string email, string passwordHash, UserRole role)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    public static Result<User> Create(string name, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<User>(Error.Problem("User.EmptyName", "Name cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return Result.Failure<User>(Error.Problem("User.InvalidEmail", "A valid email is required."));
        }

        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length < MinPasswordLength)
        {
            return Result.Failure<User>(Error.Problem("User.InvalidPassword", $"Password must be at least {MinPasswordLength} characters long."));
        }

        return new User(Guid.NewGuid(), name.Trim(), email.Trim().ToLowerInvariant(), passwordHash, role);
    }

    public Result PromoteTo(UserRole newRole)
    {
        if (Role == newRole)
        {
            return Result.Failure(Error.Problem("User.RoleAlreadyAssigned", $"User is already a {newRole}."));
        }

        Role = newRole;
        return Result.Success();
    }
}