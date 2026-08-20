using Isopoh.Cryptography.Argon2;

namespace PasswordVale.Contracts;

public interface ICryptoService
{
    public string DecryptEntry(byte[] aesKey, byte[] encryptedData);
    public byte[] DeriveAesKey(byte[] passwordBytes, byte[] salt);
    public string EncryptEntry(byte[] aesKey, byte[] plaintext);
    public Argon2Config GenerateArgon2Config(byte[] passwordBytes, byte[] saltBytes);
    public byte[] GenerateRandomSalt(int size = 16);
    public string HashMasterPassword(byte[] passwordBytes);
    public bool VerifyMasterPassword(byte[] password, byte[] passwordHash);
    public void ZeroMemory(byte[] bytesToZero);
}