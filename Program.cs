using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(optionsAction => optionsAction.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.MapGet("/todoitems", async (TodoDb db) =>
  await db.Todos.ToListAsync());

app.Run();
