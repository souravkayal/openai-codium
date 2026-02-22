using System.ComponentModel.DataAnnotations;
using TodoApp.Models;

namespace TodoApp.Tests;

public class TodoItemTests
{
    [Fact]
    public void TodoItem_WithValidData_PassesValidation()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 1,
            Title = "Test Task",
            Description = "Test Description",
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(item, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void TodoItem_WithMissingTitle_FailsValidation()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 1,
            Title = "", // Empty title - should fail
            Description = "Test Description"
        };

        // Act
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(item, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TodoItem_WithTitleExceedingLength_FailsValidation()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 1,
            Title = new string('a', 101), // Exceeds 100 character limit
            Description = "Test"
        };

        // Act
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(item, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void TodoItem_WithDescriptionExceedingLength_FailsValidation()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 1,
            Title = "Valid Title",
            Description = new string('a', 301) // Exceeds 300 character limit
        };

        // Act
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(item, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void TodoItem_WithValidTitle_PassesValidation()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 1,
            Title = new string('a', 100), // Exactly 100 characters
            Description = "Test"
        };

        // Act
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(item, context, results, true);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void TodoItem_WithNullDescription_PassesValidation()
    {
        // Arrange
        var item = new TodoItem
        {
            Id = 1,
            Title = "Test Task",
            Description = null // Description is optional
        };

        // Act
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(item, context, results, true);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void TodoItem_DefaultValues_AreSet()
    {
        // Arrange & Act
        var item = new TodoItem
        {
            Title = "Test Task"
        };

        // Assert
        Assert.Equal(0, item.Id);
        Assert.False(item.IsCompleted);
        Assert.Null(item.DueDate);
        Assert.NotEqual(DateTime.MinValue, item.CreatedAt);
    }

    [Fact]
    public void TodoItem_CreatedAt_IsSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var item = new TodoItem { Title = "Test Task" };

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.InRange(item.CreatedAt, beforeCreation, afterCreation);
    }

    [Fact]
    public void TodoItem_IsCompleted_CanBeToggled()
    {
        // Arrange
        var item = new TodoItem { Title = "Test Task", IsCompleted = false };

        // Act
        item.IsCompleted = true;

        // Assert
        Assert.True(item.IsCompleted);
    }

    [Fact]
    public void TodoItem_CanSetFutureAndPastDates()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(10);
        var pastDate = DateTime.UtcNow.AddDays(-10);

        var itemFuture = new TodoItem { Title = "Future Task", DueDate = futureDate };
        var itemPast = new TodoItem { Title = "Past Task", DueDate = pastDate };

        // Act & Assert
        Assert.Equal(futureDate, itemFuture.DueDate);
        Assert.Equal(pastDate, itemPast.DueDate);
    }
}
