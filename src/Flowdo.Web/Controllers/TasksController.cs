using System.Security.Claims;
using Flowdo.Web.Data;
using Flowdo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flowdo.Web.Controllers;

[Authorize]
[Route("tasks")]
public sealed class TasksController(
    TaskRepository repository) : Controller
{
    private Guid CurrentUserId
    {
        get
        {
            var userId = User.FindFirstValue("app_user_id");

            if (!Guid.TryParse(userId, out var id))
                throw new InvalidOperationException(
                    "Authenticated user ID is missing.");

            return id;
        }
    }

    // GET /tasks
    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken ct)
    {
        var tasks = await repository.GetAllAsync(
            CurrentUserId, ct);

        return View(tasks);
    }

    // POST /tasks/create
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromForm] CreateTaskInput input,
        CancellationToken ct)
    {
        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(input.Title))
        {
            TempData["Error"] =
                "Please enter a valid task title.";
            return RedirectToAction(nameof(Index));
        }

        await repository.CreateAsync(
            CurrentUserId,
            input.Title.Trim(),
            input.Description?.Trim(),
            ct);

        TempData["Success"] = "Task created!";
        return RedirectToAction(nameof(Index));
    }

    // GET /tasks/{id}/edit
    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(
        Guid id)
    {
        var task = await repository.GetEditableByIdAsync(
            id, CurrentUserId);

        if (task is null)
            return NotFound();

        return View(new EditTaskInput
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description
        });
    }

    // POST /tasks/{id}/edit
    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        [FromForm] EditTaskInput input)
    {
        input.Id = id;

        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(input.Title))
        {
            ModelState.AddModelError(
                nameof(input.Title),
                "Please enter a valid title.");

            return View(input);
        }

        var updated = await repository.UpdateAsync(
            id,
            CurrentUserId,
            input.Title.Trim(),
            input.Description?.Trim());

        if (!updated)
            return Conflict(
                "Task cannot be edited.");

        TempData["Success"] = "Task updated!";
        return RedirectToAction(nameof(Index));
    }

    // POST /tasks/{id}/status
    [HttpPost("{id:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromForm] string nextStatus)
    {
        if (nextStatus is not ("in_progress" or "done"))
            return BadRequest("Invalid status.");

        var updated = await repository.ChangeStatusAsync(
            id, CurrentUserId, nextStatus);

        if (!updated)
            return Conflict(
                "Status change is not allowed.");

        TempData["Success"] = "Status updated!";
        return RedirectToAction(nameof(Index));
    }

    // POST /tasks/{id}/delete
    [HttpPost("{id:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await repository.DeleteAsync(
            id, CurrentUserId);

        if (!deleted)
            return Conflict(
                "This task cannot be deleted.");

        TempData["Success"] = "Task deleted!";
        return RedirectToAction(nameof(Index));
    }
}