using PasswordVale.Contracts;

namespace PasswordVale.Models.Domain;

public class Vault(IDataService dataService)
{
    private MasterPassword? _masterPasswordRecord;

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

    public VaultState CurrentState
    {
        get;
        private set
        {
            field = value;
            OnStateChanged?.Invoke(value);
        }
    }

    public event Action<VaultState>? OnStateChanged;
}