using System.ComponentModel.DataAnnotations;

namespace Flowdo.Web.ViewModels;

public sealed class EditTaskInput
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Task title is required.")]
    [StringLength(160)]
    public string Title { get; set; } = "";

    [StringLength(3000)]
    public string? Description { get; set; }
}