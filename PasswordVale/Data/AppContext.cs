using Microsoft.EntityFrameworkCore;

namespace PasswordVale.Data;

public partial class AppContext(DbContextOptions<AppContext> options) : DbContext(options)
{
    public DbSet<MasterPassword> MasterPasswords { get; set; } = null!;
    public DbSet<PasswordEntry> PasswordEntries { get; set; } = null!;

}
