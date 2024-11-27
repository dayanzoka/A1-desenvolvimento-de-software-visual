using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>(options =>
    options.UseSqlite("Data Source=app.db")); // Verifique se o banco está configurado corretamente

var app = builder.Build();

app.MapGet("/", () => "Prova A1");

// GET: http://localhost:5273/api/categoria/listar
app.MapGet("/api/categoria/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Categorias.Any())
    {
        return Results.Ok(ctx.Categorias.ToList());
    }
    return Results.NotFound("Nenhuma categoria encontrada");
});

// POST: http://localhost:5273/api/categoria/cadastrar
app.MapPost("/api/categoria/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Categoria categoria) =>
{
    ctx.Categorias.Add(categoria);
    ctx.SaveChanges();
    return Results.Created("", categoria);
});

// GET: http://localhost:5273/api/tarefas/listar
app.MapGet("/api/tarefas/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Tarefas.Any())
    {
        return Results.Ok(ctx.Tarefas.Include(x => x.Categoria).ToList());
    }
    return Results.NotFound("Nenhuma tarefa encontrada");
});

// POST: http://localhost:5273/api/tarefas/cadastrar
app.MapPost("/api/tarefas/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Tarefa tarefa) =>
{
    Categoria? categoria = ctx.Categorias.Find(tarefa.CategoriaId);
    if (categoria == null)
    {
        return Results.NotFound("Categoria não encontrada");
    }
    tarefa.Categoria = categoria;
    tarefa.Status = "Não iniciada"; // Define o status inicial
    ctx.Tarefas.Add(tarefa);
    ctx.SaveChanges();
    return Results.Created("", tarefa);
});

// PATCH: http://localhost:5273/api/tarefa/alterar
app.MapPatch("/api/tarefa/alterar", ([FromServices] AppDataContext ctx, [FromBody] Tarefa tarefa) =>
{
    var tarefaExistente = ctx.Tarefas.Find(tarefa.TarefaId);
    if (tarefaExistente == null)
    {
        return Results.NotFound("Tarefa não encontrada");
    }

    switch (tarefaExistente.Status)
    {
        case "Não iniciada":
            tarefaExistente.Status = "Em andamento";
            break;
        case "Em andamento":
            tarefaExistente.Status = "Concluída";
            break;
        default:
            return Results.BadRequest("A tarefa já está concluída ou possui um status inválido.");
    }

    ctx.SaveChanges();
    return Results.Ok(tarefaExistente);
});

// GET: http://localhost:5273/api/tarefa/naoconcluidas
app.MapGet("/api/tarefa/naoconcluidas", ([FromServices] AppDataContext ctx) =>
{
    var tarefas = ctx.Tarefas
        .Where(t => t.Status == "Não iniciada" || t.Status == "Em andamento")
        .Include(t => t.Categoria)
        .ToList();

    if (!tarefas.Any())
    {
        return Results.NotFound("Nenhuma tarefa não concluída encontrada");
    }

    return Results.Ok(tarefas);
});

// GET: http://localhost:5273/api/tarefa/concluidas
app.MapGet("/api/tarefa/concluidas", ([FromServices] AppDataContext ctx) =>
{
    var tarefas = ctx.Tarefas
        .Where(t => t.Status == "Concluída")
        .Include(t => t.Categoria)
        .ToList();

    if (!tarefas.Any())
    {
        return Results.NotFound("Nenhuma tarefa concluída encontrada");
    }

    return Results.Ok(tarefas);
});

app.Run();
