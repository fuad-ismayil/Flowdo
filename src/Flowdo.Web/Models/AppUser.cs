namespace Flowdo.Web.Models;

public sealed class AppUser
{
    public Guid Id { get; set; }

    public string GoogleSub { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}