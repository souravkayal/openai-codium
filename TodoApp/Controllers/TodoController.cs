using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers;

public class TodoController(TodoContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await context.TodoItems
            .OrderBy(x => x.IsCompleted)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(items);
    }

    public IActionResult Create()
    {
        return View(new TodoItem { DueDate = DateTime.UtcNow.Date.AddDays(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TodoItem item)
    {
        if (!ModelState.IsValid)
        {
            return View(item);
        }

        item.CreatedAt = DateTime.UtcNow;
        context.Add(item);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await context.TodoItems.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TodoItem item)
    {
        if (id != item.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(item);
        }

        context.Update(item);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await context.TodoItems.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await context.TodoItems.FindAsync(id);
        if (item is not null)
        {
            context.TodoItems.Remove(item);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        var item = await context.TodoItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsCompleted = !item.IsCompleted;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
