namespace PasswordVale.Models.Domain;

/// <summary>
/// PasswordEntrySummary includes details for a password entry, but omits
/// anything related to the actual password. It is assumed that if the raw
/// password is needed, it will be fetched by other means.
/// </summary>
public class PasswordEntrySummary
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string? Username { get; set; }
    public required bool Favorite { get; set; }
}
