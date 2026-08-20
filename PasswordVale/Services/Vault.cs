using PasswordVale.Contracts;

namespace PasswordVale.Services;

/// <summary>
/// The password vault which manages application state.
/// </summary>
public class Vault(IDataService dataService)
{
    private MasterPassword? _masterPasswordRecord;

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
    public async Task SetMasterPassword(string password)
    {
        // TODO: Exception handling
        // TODO: proper hashing
        _ = password;

        var id = Guid.NewGuid();

        var pw = new MasterPassword()
        {
            Id = id,
            PasswordHash = Convert.FromBase64String($"Hash For {id}"),
            AesEncryptionKeySalt = Convert.FromBase64String($"Salt for {id}"),
        };


        await dataService.CreateMasterPassword(pw);
        _masterPasswordRecord = pw;
        CurrentState = VaultState.Locked;
    }
}