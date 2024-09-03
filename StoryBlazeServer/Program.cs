using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using StoryBlazeServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using StoryBlazeServer.Models;
using StoryBlazeServer.Controllers;
using WEBAPIGMINGENIEROSHTTPS.Custom;
using WEBAPIGMINGENIEROSHTTPS.Models.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la cadena de conexión
var cadcn = builder.Configuration.GetConnectionString("cn1");
builder.Services.AddDbContext<StoryBlazeServerContext>(opt => opt.UseSqlServer(cadcn));

// Registro de servicios
builder.Services.AddSingleton<Utilidades>();

// Servicio de correo
builder.Services.AddSingleton(new EmailService(
    smtpServer: "smtp.gmail.com",
    smtpPort: 587,
    smtpUser: "alenaguilar24@gmail.com",
    smtpPass: "krvd ajsr ruuf fwgj"
));

// Controlador de acceso (deberías agregar todos los controladores relevantes)
builder.Services.AddScoped<AccesoController>();
builder.Services.AddHttpClient<HistoriaService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7107/");
});


// Configuración de autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(config =>
    {
        config.RequireHttpsMetadata = false;
        config.SaveToken = true;
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

// Configuración de Swagger para la documentación de la API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StoryBlazeServer API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingrese su Token JWT en el formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Configuración de servicios de Blazor y Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configuración de CORS
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Configuración de Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "StoryBlazeServer API V1");
    c.DefaultModelsExpandDepth(-1);
    c.DefaultModelExpandDepth(-1);
});

// Configuración del entorno
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de controladores
app.MapControllers();

// Configuración de Blazor Server
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
