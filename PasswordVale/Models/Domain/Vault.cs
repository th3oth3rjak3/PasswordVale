namespace PasswordVale.Models.Domain;

public class Vault
{
    public MasterPassword? MasterPasswordRecord
    {
        get;
        set
        {
            field = value;
            OnMasterPasswordChange?.Invoke(value);
        }
    }

    public event Action<MasterPassword?>? OnMasterPasswordChange;

    public bool Locked { get; private set; } = true;
}
