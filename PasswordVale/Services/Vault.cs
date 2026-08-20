using PasswordVale.Contracts;

namespace PasswordVale.Services;

/// <summary>
/// The password vault which manages application state.
/// </summary>
public class Vault(IDataService dataService, ICryptoService cryptoService)
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
}