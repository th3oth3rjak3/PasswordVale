namespace PasswordVale.Contracts;

public interface ICryptoService
{
    public string DecryptEntry(byte[] aesKey, byte[] encryptedData);
    public byte[] DeriveAesKey(byte[] passwordBytes, byte[] salt);
    public byte[] EncryptEntry(byte[] aesKey, string plaintext);
    public byte[] GenerateRandomSalt(int size = 16);
    public byte[] HashMasterPassword(byte[] passwordBytes);
    public bool VerifyMasterPassword(byte[] password, byte[] passwordHash);
    public void ZeroMemory(byte[] bytesToZero);
}