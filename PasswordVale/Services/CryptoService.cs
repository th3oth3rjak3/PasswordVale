using System.Security.Cryptography;
using System.Text;

using Isopoh.Cryptography.Argon2;

using PasswordVale.Contracts;

namespace PasswordVale.Services;

public class CryptoService : ICryptoService
{
    /// <summary>
    /// The number of iterations (time cost) for the Argon2id hashing algorithm.
    /// </summary>
    private const int TimeCost = 4;

    /// <summary>
    /// The target count of memory blocks to be used by the Argon2id hashing algorithm.
    /// </summary>
    private const int MemoryCost = 1 << 16;

    /// <summary>
    /// The degree of parallelism (number of lanes) for the Argon2id hashing algorithm.
    /// </summary>
    private static readonly int Parallelism = Environment.ProcessorCount;

    /// <summary>
    /// Represents the fixed length, in bytes, of the hash value used in the application.
    /// </summary>
    private const int HashLength = 32;


    /// <summary>
    /// Generates a memory hard Argon2id hash of the master password.
    /// This should be used when the password is first created or changed before
    /// storing it in the database.
    /// </summary>
    /// <param name="passwordBytes">The raw master password input from the user.</param>
    /// <returns>The Argon2id hash.</returns>
    public string HashMasterPassword(byte[] passwordBytes)
    {
        var saltBytes = GenerateRandomSalt();
        var config = GenerateArgon2Config(passwordBytes, saltBytes);
        using var argon2 = new Argon2(config);
        var hash = Argon2.Hash(config);
        CryptographicOperations.ZeroMemory(passwordBytes);
        return hash;
    }

    /// <summary>
    /// Verifies whether the provided password matches the specified password hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="passwordHash">The hashed password to compare against.</param>
    /// <returns>True when valid, otherwise false.</returns>
    public bool VerifyMasterPassword(byte[] password, byte[] passwordHash) =>
        Argon2.Verify(Convert.ToBase64String(passwordHash), password);

    /// <summary>
    /// Derive an AES-256 encryption key from the master password using Argon2id.
    /// </summary>
    /// <param name="passwordBytes">The master password.</param>
    /// <param name="salt">The key derivation salt stored with the master password hash.</param>
    /// <returns>An AES encryption key to be used when encrypting and decrypting password entries.</returns>
    public byte[] DeriveAesKey(byte[] passwordBytes, byte[] salt)
    {
        var config = GenerateArgon2Config(passwordBytes, salt);
        using var argon2 = new Argon2(config);
        var buffer = argon2.Hash().Buffer;
        ZeroMemory(passwordBytes);
        return buffer;
    }

    /// <summary>
    /// Generates a cryptographically secure random salt.
    /// </summary>
    /// <param name="size">The size in bytes for the salt.</param>
    /// <returns>The random salt bytes.</returns>
    public byte[] GenerateRandomSalt(int size = 16)
    {
        var salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    /// <summary>
    /// Generates an Argon2 configuration object with predefined parameters for secure password hashing.
    /// </summary>
    /// <remarks>This method uses the Argon2id variant, which provides resistance to both GPU-based attacks
    /// and side-channel attacks. The configuration is suitable for secure password storage, with parameters optimized
    /// for a balance between security and performance.</remarks>
    /// <param name="passwordBytes">The password to be hashed, provided as utf8 bytes. Cannot be null or empty.</param>
    /// <param name="saltBytes">The salt to be used in the hashing process, provided as a byte array. Cannot be null or empty.</param>
    /// <returns>An <see cref="Argon2Config"/> object configured with Argon2id mode, a time cost of 4 iterations, a memory cost
    /// of 64 MiB, 4 lanes for parallelism, and a hash length of 32 bytes.</returns>
    public Argon2Config GenerateArgon2Config(byte[] passwordBytes, byte[] saltBytes) =>
        new()
        {
            Type = Argon2Type.DataIndependentAddressing, // Argon2id
            Version = Argon2Version.Nineteen,
            TimeCost = TimeCost,
            MemoryCost = MemoryCost,
            Lanes = Parallelism,
            Threads = Parallelism,
            Password = passwordBytes,
            Salt = saltBytes,
            HashLength = HashLength,
        };

    private const int NonceSize = 12; // Recommended size for GCM nonce
    private const int TagSize = 16;   // 128-bit auth tag

    /// <summary>
    /// Encrypts the specified plaintext using AES-GCM encryption with the provided key.
    /// </summary>
    /// <remarks>The method uses AES-GCM (Galois/Counter Mode) for encryption, which provides both
    /// confidentiality and integrity. The returned Base64-encoded string includes the 12-byte nonce, 16-byte
    /// authentication tag, and the ciphertext. The caller is responsible for securely storing the AES key, as it is
    /// required for decryption.</remarks>
    /// <param name="aesKey">A 256-bit AES key used for encryption. The key must be exactly 32 bytes in length.</param>
    /// <param name="plaintext">The plaintext string to encrypt. This value cannot be <see langword="null"/> or empty.</param>
    /// <returns>A Base64-encoded string containing the encrypted data, which includes the nonce, authentication tag, and
    /// ciphertext.</returns>
    public string EncryptEntry(byte[] aesKey, byte[] plaintext)
    {
        if (aesKey.Length != 32)
            throw new ArgumentException("AES key must be 256 bits (32 bytes).", nameof(aesKey));
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintext.Length];

        using var aesGcm = new AesGcm(aesKey, TagSize);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        var encrypted = Convert.ToBase64String([.. nonce, .. tag, .. ciphertext]);
        ZeroMemory(plaintext);
        return encrypted;
    }

    /// <summary>
    /// Decrypts the specified encrypted data using the provided AES key.
    /// </summary>
    /// <remarks>This method expects the encrypted data to be formatted as a Base64-encoded string containing
    /// the nonce, authentication tag, and ciphertext in sequence. The nonce and tag sizes must match the expected
    /// values for the AES-GCM encryption scheme.</remarks>
    /// <param name="aesKey">The AES key used for decryption. The key must be a valid size for AES encryption (e.g., 128, 192, or 256 bits).</param>
    /// <param name="encryptedData">The encrypted data as a Base64-encoded string. The data must include the nonce, authentication tag, and
    /// ciphertext in sequence.</param>
    /// <returns>The decrypted plaintext as a UTF-8 encoded string.</returns>
    public string DecryptEntry(byte[] aesKey, byte[] encryptedData)
    {
        if (aesKey.Length != 32)
            throw new ArgumentException("AES key must be 256 bits (32 bytes).", nameof(aesKey));

        if (encryptedData.Length < NonceSize + TagSize)
            throw new CryptographicException("Encrypted data is too short or corrupted.");

        var nonce = encryptedData.AsSpan(0, NonceSize);
        var tag = encryptedData.AsSpan(NonceSize, TagSize);
        var ciphertext = encryptedData.AsSpan(NonceSize + TagSize);

        var plaintextBytes = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(aesKey, TagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);

        var plaintext = Encoding.UTF8.GetString(plaintextBytes);
        ZeroMemory(plaintextBytes);
        return plaintext;
    }

    public void ZeroMemory(byte[] bytes)
    {
        CryptographicOperations.ZeroMemory(bytes);
    }
}