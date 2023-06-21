using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(optionsAction => optionsAction.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllAsync);
todoItems.MapGet("/complete", GetCompleteAsync);
todoItems.MapGet("/{id}", GetTodoAsync);
todoItems.MapPost("/", CreateTodoAsync);
todoItems.MapPut("/{id}", UpdateTodoAsync);
todoItems.MapDelete("/{id}", DeleteTodoAsync);

app.Run();


static async Task<IResult> GetAllAsync(TodoDb db)
{
  return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

static async Task<IResult> GetCompleteAsync(TodoDb db)
{
  return TypedResults.Ok(await db.Todos.Where(x => x.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<IResult> GetTodoAsync(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodoAsync(TodoItemDTO todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

static async Task<IResult> UpdateTodoAsync(int id, TodoItemDTO inputTodoDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = inputTodoDTO.Name;
    todo.IsComplete = inputTodoDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodoAsync(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        TodoItemDTO todoItemDTO = new(todo);

        return TypedResults.Ok(todoItemDTO);
    }

    return TypedResults.NotFound();
}