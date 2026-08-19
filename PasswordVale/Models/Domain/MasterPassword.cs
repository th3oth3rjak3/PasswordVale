namespace PasswordVale.Models.Domain;

public class MasterPassword
{
    public required Guid Id { get; set; }
    public required string PasswordHash { get; set; }
    public required string AesEncryptionKeySalt { get; set; }
}
