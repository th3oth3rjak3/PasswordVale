using PasswordVale.Contracts;
using PasswordVale.Models.Transfer;

namespace PasswordVale.Services;

public class FakeDataService : IDataService
{
    private MasterPassword? _masterPassword;
    private readonly List<PasswordEntry> _passwordEntries = [];

    public Task CreateMasterPassword(MasterPassword masterPassword)
    {
        _masterPassword = masterPassword;

        return Task.CompletedTask;
    }


    public Task CreatePasswordEntry(PasswordEntryWrite newEntry)
    {
        var newItem = new PasswordEntry()
        {
            Id = Guid.NewGuid(),
            Name = newEntry.Name ?? "",
            EncryptedPassword = newEntry.EncryptedPassword,
            Username = newEntry.Username ?? "",
            Favorite = newEntry.Favorite,
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
        Task.FromResult(_passwordEntries.Select(pw => new PasswordEntrySummary()
        {
            Id = pw.Id,
            Username = pw.Username,
            Name = pw.Name,
            Favorite = pw.Favorite,
        })
        .ToList());

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
        found.Username = updatedEntry.Username ?? "";
        found.EncryptedPassword = updatedEntry.EncryptedPassword;

        return Task.CompletedTask;
    }

    public Task<PasswordEntry?> GetPasswordEntryRaw(Guid id) =>
        Task.FromResult(_passwordEntries.SingleOrDefault(pw => pw.Id == id));
}
