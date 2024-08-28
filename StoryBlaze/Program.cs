using StoryBlaze.Models;
using Microsoft.EntityFrameworkCore;
using StoryBlaze.Controllers;
using WEBAPIGMINGENIEROSHTTPS.Custom;
using WEBAPIGMINGENIEROSHTTPS.Models.Services;


var builder = WebApplication.CreateBuilder(args);
var cadcn = builder.Configuration.GetConnectionString("cn1");
builder.Services.AddDbContext<StoryBlazeContext>(
    opt => opt.UseSqlServer(cadcn));

// Registro de servicios como singleton o scoped
builder.Services.AddSingleton<Utilidades>();

builder.Services.AddSingleton(new EmailService(
    smtpServer: "smtp.gmail.com",
    smtpPort: 587,
    smtpUser: "alenaguilar24@gmail.com",
    smtpPass: "krvd ajsr ruuf fwgj"
));

// Registrar el controlador como un servicio
builder.Services.AddScoped<AccesoController>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
