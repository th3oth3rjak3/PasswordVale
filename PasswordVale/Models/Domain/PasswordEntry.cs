namespace PasswordVale.Models.Domain;

public class PasswordEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Username { get; set; }
    public required byte[] EncryptedPassword { get; set; }
}
