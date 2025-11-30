using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הגדרת פורט האזנה עבור Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// הוספת שירותים ל-container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוספת שירות CORS כדי שהריאקט יוכל לדבר עם השרת
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// שליפת מחרוזת החיבור
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");

// הגדרת ה-DB עם גרסה קבועה (במקום זיהוי אוטומטי שגורם לקריסות)
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    // אם אין מחרוזת חיבור (למשל שכחנו להגדיר ב-Render), נשתמש במחרוזת ריקה כדי לא לקרוס מיד,
    // אבל ה-DB לא יעבוד עד שנגדיר את המשתנה ב-Render.
    var connStr = connectionString ?? "Server=localhost;Database=test;User=root;Password=root;";

    options.UseMySql(connStr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql"));
});

var app = builder.Build();

// הפעלת CORS
app.UseCors("AllowAll");

// הגדרות Swagger (נשאיר גם בפרודקשן כדי שתוכלי לבדוק שזה עובד)
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection(); // ב-Render לעיתים זה מפריע אם אין תעודה מוגדרת, נשאיר כרגע בהערה או נבטל אם עושה בעיות

// הגדרת ה-Routes
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

app.Run();