using PasswordVale.Contracts;

namespace PasswordVale.Models.Domain;

public class Vault(IDataService dataService)
{
    public async Task Initialize()
    {
        var masterPwRecord = await dataService.GetMasterPassword();
        if (masterPwRecord is null)
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