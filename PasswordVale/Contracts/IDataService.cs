using PasswordVale.Models.Transfer;

namespace PasswordVale.Contracts;

/// <summary>
/// A service that manages the stored data for the application.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Create a new master password record. This method will throw an exception if one already exists.
    /// </summary>
    /// <param name="masterPassword">The hashed master password record used to unlock the vault.</param>
    public Task CreateMasterPassword(MasterPassword masterPassword);

    /// <summary>
    /// GetMasterPassword attempts to fetch a master password record. It assumes that there
    /// will be zero or one master password records. If multiple entries are detected, it will
    /// throw an exception.
    /// </summary>
    public Task<MasterPassword?> GetMasterPassword();

    /// <summary>
    /// GetAllPasswordEntries fetches all of the stored password entries.
    /// </summary>
    public Task<List<PasswordEntrySummary>> GetAllPasswordEntries();

    /// <summary>
    /// Create a new password entry.
    /// </summary>
    /// <param name="newEntry">The details for the new entry.</param>
    public Task CreatePasswordEntry(PasswordEntryWrite newEntry);

    /// <summary>
    /// Update a password entry.
    /// </summary>
    /// <param name="id">The unique id of the entry to update.</param>
    /// <param name="updatedEntry">Updated details for the entry.</param>
    public Task UpdatePasswordEntry(Guid id, PasswordEntryWrite updatedEntry);

    /// <summary>
    /// Delete an existing password entry.
    /// </summary>
    /// <param name="id">The unique id of the entry to delete.</param>
    public Task DeletePasswordEntry(Guid id);
}
