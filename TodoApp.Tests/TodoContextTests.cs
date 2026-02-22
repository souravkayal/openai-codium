using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Tests;

public class TodoContextTests
{
    private TodoContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TodoContext(options);
    }

    [Fact]
    public async Task TodoContext_CanAddTodoItem()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var item = new TodoItem { Title = "Test Item", Description = "Test" };

        // Act
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Assert
        Assert.NotEqual(0, item.Id); // EF should assign an ID
        var savedItem = await context.TodoItems.FirstOrDefaultAsync();
        Assert.NotNull(savedItem);
        Assert.Equal("Test Item", savedItem.Title);
    }

    [Fact]
    public async Task TodoContext_CanUpdateTodoItem()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var item = new TodoItem { Title = "Original", IsCompleted = false };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        var itemId = item.Id;

        // Act
        var retrievedItem = await context.TodoItems.FindAsync(itemId);
        if (retrievedItem != null)
        {
            retrievedItem.Title = "Updated";
            retrievedItem.IsCompleted = true;
            await context.SaveChangesAsync();
        }

        // Assert
        var updatedItem = await context.TodoItems.FindAsync(itemId);
        Assert.NotNull(updatedItem);
        Assert.Equal("Updated", updatedItem.Title);
        Assert.True(updatedItem.IsCompleted);
    }

    [Fact]
    public async Task TodoContext_CanDeleteTodoItem()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var item = new TodoItem { Title = "Item to Delete" };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        var itemId = item.Id;

        // Act
        var itemToDelete = await context.TodoItems.FindAsync(itemId);
        if (itemToDelete != null)
        {
            context.TodoItems.Remove(itemToDelete);
            await context.SaveChangesAsync();
        }

        // Assert
        var deletedItem = await context.TodoItems.FindAsync(itemId);
        Assert.Null(deletedItem);
    }

    [Fact]
    public async Task TodoContext_CanRetrieveAllTodoItems()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var items = new List<TodoItem>
        {
            new TodoItem { Title = "Item 1" },
            new TodoItem { Title = "Item 2" },
            new TodoItem { Title = "Item 3" }
        };

        foreach (var item in items)
        {
            context.TodoItems.Add(item);
        }
        await context.SaveChangesAsync();

        // Act
        var allItems = await context.TodoItems.ToListAsync();

        // Assert
        Assert.Equal(3, allItems.Count);
    }

    [Fact]
    public async Task TodoContext_TodoItems_Property_IsAccessible()
    {
        // Arrange
        var context = CreateInMemoryContext();

        // Act
        var dbSet = context.TodoItems;

        // Assert
        Assert.NotNull(dbSet);
    }

    [Fact]
    public async Task TodoContext_CanQueryByIsCompleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var items = new List<TodoItem>
        {
            new TodoItem { Title = "Completed", IsCompleted = true },
            new TodoItem { Title = "Incomplete 1", IsCompleted = false },
            new TodoItem { Title = "Incomplete 2", IsCompleted = false }
        };

        foreach (var item in items)
        {
            context.TodoItems.Add(item);
        }
        await context.SaveChangesAsync();

        // Act
        var completedItems = await context.TodoItems.Where(x => x.IsCompleted).ToListAsync();
        var incompleteItems = await context.TodoItems.Where(x => !x.IsCompleted).ToListAsync();

        // Assert
        Assert.Single(completedItems);
        Assert.Equal(2, incompleteItems.Count);
    }

    [Fact]
    public async Task TodoContext_CanQueryByDueDate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var today = DateTime.UtcNow.Date;
        var items = new List<TodoItem>
        {
            new TodoItem { Title = "Today Task", DueDate = today },
            new TodoItem { Title = "Tomorrow Task", DueDate = today.AddDays(1) },
            new TodoItem { Title = "Yesterday Task", DueDate = today.AddDays(-1) }
        };

        foreach (var item in items)
        {
            context.TodoItems.Add(item);
        }
        await context.SaveChangesAsync();

        // Act
        var upcomingTasks = await context.TodoItems
            .Where(x => x.DueDate.HasValue && x.DueDate >= today)
            .ToListAsync();

        // Assert
        Assert.Equal(2, upcomingTasks.Count);
    }

    [Fact]
    public async Task TodoContext_PreservesCreatedAtOnUpdate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var originalCreatedAt = DateTime.UtcNow.AddDays(-5);
        var item = new TodoItem 
        { 
            Title = "Original", 
            CreatedAt = originalCreatedAt 
        };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        var itemId = item.Id;

        // Act
        var retrievedItem = await context.TodoItems.FindAsync(itemId);
        if (retrievedItem != null)
        {
            retrievedItem.Title = "Updated";
            await context.SaveChangesAsync();
        }

        // Assert
        var updatedItem = await context.TodoItems.FindAsync(itemId);
        Assert.NotNull(updatedItem);
        Assert.Equal(originalCreatedAt, updatedItem.CreatedAt);
    }
}
