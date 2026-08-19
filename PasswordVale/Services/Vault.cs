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
}