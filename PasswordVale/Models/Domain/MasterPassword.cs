namespace PasswordVale.Models.Domain;

public class MasterPassword
{
    public required Guid Id { get; set; }
    public required byte[] PasswordHash { get; set; }
    public required byte[] AesEncryptionKeySalt { get; set; }
}
