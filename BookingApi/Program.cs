using BookingApi.Data;
using BookingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddDbContext<BookingApi.Data.AppDbContext>(o =>o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=booking.db"));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingApi.Data.AppDbContext>();
    db.Database.Migrate();
    BookingApi.Data.SeedData.Run(db);
}

/* Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}*
*/

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }