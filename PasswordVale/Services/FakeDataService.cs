using PasswordVale.Contracts;
using PasswordVale.Models.Transfer;

namespace PasswordVale.Services;

public class FakeDataService : IDataService
{
    private MasterPassword? _masterPassword;
    private readonly List<PasswordEntrySummary> _passwordEntries = [];

    public Task<MasterPassword> CreateMasterPassword(string masterPassword)
    {
        // Just throw it away since it's a fake.
        _ = masterPassword;

        var id = Guid.NewGuid();

        _masterPassword = new MasterPassword()
        {
            Id = id,
            PasswordHash = $"Hash For {id}",
            AesEncryptionKeySalt = $"Salt for {id}",
        };

        return Task.FromResult(_masterPassword);
    }


    public Task CreatePasswordEntry(PasswordEntryWrite newEntry)
    {
        var newItem = new PasswordEntrySummary()
        {
            Id = Guid.NewGuid(),
            Name = newEntry.Name ?? "",
            Username = newEntry.Username,
            Favorite = newEntry.Favorite,
            Tags = newEntry.Tags,
        };

        _passwordEntries.Add(newItem);
        return Task.CompletedTask;
    }

    public Task DeletePasswordEntry(Guid id)
    {
        var item = _passwordEntries.FirstOrDefault(item => item.Id == id);
        if (item is null)
            return Task.CompletedTask;

        _passwordEntries.Remove(item);

        return Task.CompletedTask;
    }

    public Task<List<PasswordEntrySummary>> GetAllPasswordEntries() =>
        Task.FromResult(_passwordEntries);

    public Task<MasterPassword?> GetMasterPassword() =>
        Task.FromResult(_masterPassword);

    public Task UpdatePasswordEntry(Guid id, PasswordEntryWrite updatedEntry)
    {
        var found = _passwordEntries.SingleOrDefault(pw => pw.Id == id);
        if (found is null)
        {
            return Task.CompletedTask;
        }

        found.Favorite = updatedEntry.Favorite;
        found.Username = updatedEntry.Username;
        found.Tags.Clear();
        found.Tags.AddRange(updatedEntry.Tags);

        return Task.CompletedTask;
    }
}
