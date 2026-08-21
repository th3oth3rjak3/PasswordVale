using PasswordVale.Contracts;

namespace PasswordVale.Services;

/// <summary>
/// The password vault which manages application state.
/// </summary>
public class Vault(IDataService dataService, ICryptoService cryptoService)
{
    private MasterPassword? _masterPasswordRecord;
    private byte[]? _sessionAesKey;

    /// <summary>
    /// Initialize the vault. This should only be done once when the application starts.
    /// </summary>
    public async Task Initialize()
    {
        _masterPasswordRecord = await dataService.GetMasterPassword();
        if (_masterPasswordRecord is null)
        {
            CurrentState = VaultState.NotConfigured;
            return;
        }

        CurrentState = VaultState.Locked;
    }

    /// <summary>
    /// The current state of the vault.
    /// </summary>
    public VaultState CurrentState
    {
        get;
        private set
        {
            field = value;
            OnStateChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Called when CurrentState changes.
    /// </summary>
    public event Action<VaultState>? OnStateChanged;

    /// <summary>
    /// Set the master password for the vault.
    /// </summary>
    /// <param name="password">The master password that unlocks the vault.</param>
    public async Task SetMasterPassword(byte[] password)
    {
        var pw = cryptoService.HashMasterPassword(password);
        var salt = cryptoService.GenerateRandomSalt();

        var masterPw = new MasterPassword
        {
            Id = Guid.NewGuid(),
            PasswordHash = pw,
            AesEncryptionKeySalt = salt,
        };

        await dataService.CreateMasterPassword(masterPw);
        _masterPasswordRecord = masterPw;

        cryptoService.ZeroMemory(password);

        // TODO: decide if being auto-logged in after setting the pw is a better user experience.
        // We could choose to let the user be "logged in" already
        // since they just gave us their password. Logging in forces
        // them to remember the password though, so it might be useful.
        CurrentState = VaultState.Locked;
    }

    /// <summary>
    /// Unlock the vault using the master password.
    /// </summary>
    /// <param name="password">The master password.</param>
    public async Task Unlock(byte[] password)
    {
        try
        {
            if (_masterPasswordRecord is null)
                throw new InvalidOperationException("Vault is not setup");

            if (cryptoService.VerifyMasterPassword(password, _masterPasswordRecord.PasswordHash))
            {
                _sessionAesKey = cryptoService.DeriveAesKey(password, _masterPasswordRecord.AesEncryptionKeySalt);
                CurrentState = VaultState.Unlocked;
                return;
            }

            throw new InvalidOperationException("Incorrect Password");
        }
        finally
        {
            cryptoService.ZeroMemory(password);
        }
    }

    /// <summary>
    /// Locks the vault and immediately wipes the session AES key from memory.
    /// </summary>
    public void Lock()
    {
        if (_sessionAesKey != null)
        {
            cryptoService.ZeroMemory(_sessionAesKey);
            _sessionAesKey = null;
        }

        CurrentState = VaultState.Locked;
    }

    /// <summary>
    /// Decrypts a specific entry on-demand using the active session key.
    /// </summary>
    public async Task<string> GetDecryptedPassword(Guid id)
    {
        if (CurrentState != VaultState.Unlocked || _sessionAesKey == null)
            throw new InvalidOperationException("Vault is locked.");

        // 1. Fetch the raw encrypted bytes [nonce + tag + ciphertext] from DataService
        var entry = await dataService.GetPasswordEntryRaw(id);
        if (entry == null)
            throw new KeyNotFoundException("Entry not found.");

        // 2. Decrypt on-demand
        return cryptoService.DecryptEntry(_sessionAesKey, entry.EncryptedPassword);
    }
}