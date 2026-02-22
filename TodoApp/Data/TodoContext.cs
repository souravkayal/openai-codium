using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
}
