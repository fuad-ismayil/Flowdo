namespace Flowdo.Web.Models;

public sealed class TaskItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public string Status { get; set; } = "todo";
    public bool HasStarted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}