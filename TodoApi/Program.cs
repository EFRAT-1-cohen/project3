using System;
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// --- הגדרות פורטים עבור Render ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// --- הוספת שירותים ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- הגדרת CORS (כדי שהריאקט יוכל להתחבר) ---
// --- הגדרת CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins(
                    "https://project3-lo50.onrender.com",
                    "http://localhost:3000"
                )
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// --- חיבור למסד הנתונים ---
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");

builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    var connStr = connectionString ?? "Server=localhost;Database=test;User=root;Password=root;";
    options.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 2)));
});

var app = builder.Build();

// --- קריטי: CORS חייב להיות לפני כל השאר! ---
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

// --- הגדרת נתיבים (Routes) ---
var apiRoutes = app.MapGroup("/api/items");

apiRoutes.MapGet("/", async (ToDoDbContext db) =>
{
    return Results.Ok(await db.Items.ToListAsync());
});

apiRoutes.MapPost("/", async (Item item, ToDoDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{item.Id}", item);
});

apiRoutes.MapPut("/{id}", async (int id, Item inputItem, ToDoDbContext db) =>
{
    var itemToUpdate = await db.Items.FindAsync(id);
    if (itemToUpdate == null) return Results.NotFound();

    itemToUpdate.Name = inputItem.Name;
    itemToUpdate.IsComplete = inputItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

apiRoutes.MapDelete("/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// הפניה מהשורש ל-Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();