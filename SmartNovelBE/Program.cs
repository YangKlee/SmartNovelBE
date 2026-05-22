using SmartNovelBE.Models;
using Microsoft.EntityFrameworkCore;    
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var conString = builder.Configuration.GetConnectionString("SmartNovel");
builder.Services.AddDbContext<SmartTruyenDbContext>(options =>
    options.UseSqlServer(conString));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
