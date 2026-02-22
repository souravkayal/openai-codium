using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class TodoItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
