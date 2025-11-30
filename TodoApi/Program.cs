using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// 1. הוספת שירותים ל-container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. הגדרת ה-DB
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// 3. הגדרת CORS בצורה נכונה (פעם אחת בלבד!)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // מאפשר גישה מכל מקום (פותר את הבעיה ב-Render)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 4. הפעלת ה-CORS (חייב להיות לפני ה-Routes!)
app.UseCors("AllowAll");

// הגדרות Swagger ופיתוח
// (במקרה שלך כדאי להשאיר את ה-Swagger זמין גם ב-Production לצורך בדיקות אם תרצי)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 5. הגדרת ה-Routes
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
    var itemToDelete = await db.Items.FindAsync(id);
    if (itemToDelete != null)
    {
        db.Items.Remove(itemToDelete);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.MapGet("/", () => "Welcome to the ToDo API!");

app.Run();