using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Controllers;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Tests;

public class TodoControllerTests
{
    private TodoContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TodoContext(options);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfTodoItems()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);

        var todoItems = new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Item 1", IsCompleted = false, CreatedAt = DateTime.UtcNow },
            new TodoItem { Id = 2, Title = "Item 2", IsCompleted = true, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        };

        foreach (var item in todoItems)
        {
            context.TodoItems.Add(item);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedItems = Assert.IsAssignableFrom<List<TodoItem>>(viewResult.Model);
        Assert.Equal(2, returnedItems.Count);
    }

    [Fact]
    public async Task Index_ReturnsSortedItems_IncompleteFirst()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);

        var todoItems = new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Completed", IsCompleted = true, CreatedAt = DateTime.UtcNow },
            new TodoItem { Id = 2, Title = "Incomplete", IsCompleted = false, CreatedAt = DateTime.UtcNow }
        };

        foreach (var item in todoItems)
        {
            context.TodoItems.Add(item);
        }
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedItems = Assert.IsAssignableFrom<List<TodoItem>>(viewResult.Model);
        Assert.False(returnedItems.First().IsCompleted);
        Assert.True(returnedItems.Last().IsCompleted);
    }

    [Fact]
    public void Create_Get_ReturnsViewResult_WithNewTodoItem()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var todoItem = Assert.IsType<TodoItem>(viewResult.Model);
        Assert.NotNull(todoItem.DueDate);
    }

    [Fact]
    public async Task Create_Post_WithValidModel_RedirectsToIndex()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var newItem = new TodoItem { Title = "New Task", Description = "Test" };

        // Act
        var result = await controller.Create(newItem);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodoController.Index), redirectResult.ActionName);
        Assert.Single(context.TodoItems);
    }

    [Fact]
    public async Task Create_Post_WithInvalidModel_ReturnsView()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var newItem = new TodoItem { Title = "" }; // Invalid - title is required

        controller.ModelState.AddModelError("Title", "Required");

        // Act
        var result = await controller.Create(newItem);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(newItem, viewResult.Model);
    }

    [Fact]
    public async Task Edit_Get_WithValidId_ReturnsViewResult()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var item = new TodoItem { Id = 1, Title = "Test Item" };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedItem = Assert.IsType<TodoItem>(viewResult.Model);
        Assert.Equal(1, returnedItem.Id);
        Assert.Equal("Test Item", returnedItem.Title);
    }

    [Fact]
    public async Task Edit_Get_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);

        // Act
        var result = await controller.Edit(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_WithMatchingId_RedirectsToIndex()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var item = new TodoItem { Id = 1, Title = "Original", Description = "Original Description" };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Detach the item so we can update it with a new instance
        context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var updatedItem = new TodoItem { Id = 1, Title = "Updated", Description = "Updated Description" };

        // Act
        var result = await controller.Edit(1, updatedItem);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodoController.Index), redirectResult.ActionName);

        var savedItem = await context.TodoItems.FindAsync(1);
        Assert.Equal("Updated", savedItem?.Title);
    }

    [Fact]
    public async Task Edit_Post_WithMismatchedId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var updatedItem = new TodoItem { Id = 2, Title = "Updated" };

        // Act
        var result = await controller.Edit(1, updatedItem);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_WithValidId_ReturnsViewResult()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var item = new TodoItem { Id = 1, Title = "Test Item" };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Delete(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedItem = Assert.IsType<TodoItem>(viewResult.Model);
        Assert.Equal(1, returnedItem.Id);
    }

    [Fact]
    public async Task Delete_Get_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);

        // Act
        var result = await controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_WithValidId_RemovesItemAndRedirects()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var item = new TodoItem { Id = 1, Title = "Test Item" };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodoController.Index), redirectResult.ActionName);
        Assert.Empty(context.TodoItems);
    }

    [Fact]
    public async Task DeleteConfirmed_WithInvalidId_StillRedirects()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);

        // Act
        var result = await controller.DeleteConfirmed(999);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodoController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task ToggleComplete_WithValidId_TogglesStatus()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var item = new TodoItem { Id = 1, Title = "Test Item", IsCompleted = false };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.ToggleComplete(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TodoController.Index), redirectResult.ActionName);

        var savedItem = await context.TodoItems.FindAsync(1);
        Assert.True(savedItem?.IsCompleted);
    }

    [Fact]
    public async Task ToggleComplete_WithValidId_TogglesTrueToFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);
        var item = new TodoItem { Id = 1, Title = "Test Item", IsCompleted = true };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.ToggleComplete(1);

        // Assert
        var savedItem = await context.TodoItems.FindAsync(1);
        Assert.False(savedItem?.IsCompleted);
    }

    [Fact]
    public async Task ToggleComplete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var controller = new TodoController(context);

        // Act
        var result = await controller.ToggleComplete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
