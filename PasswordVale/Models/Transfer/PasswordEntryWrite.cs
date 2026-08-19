using System.ComponentModel.DataAnnotations;

namespace PasswordVale.Models.Transfer;

public class PasswordEntryWrite
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }

    public bool Favorite { get; set; }
    public List<string> Tags { get; set; } = [];
}
