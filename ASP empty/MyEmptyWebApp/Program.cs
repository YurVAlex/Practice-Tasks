var builder = WebApplication.CreateBuilder();
var app = builder.Build();
 
app.MapGet("/{id?}", (int? id) =>
{
    if (id is null)
        return Results.BadRequest(new { Message = "Некорректные данные в запросе" });
    else if (id != 1)
        return Results.NotFound(new { Message = $"Объект с id={id} не существует" });
    else
        return Results.Json(new Person("Bob", 42));
});
 
app.Run();
record Person(string Name, int Age);