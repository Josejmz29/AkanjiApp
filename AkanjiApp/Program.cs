using AkanjiApp.Controllers;
using AkanjiApp.Models;
using AkanjiApp.Repository;
using AkanjiApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Agregar contexto de base de datos MySQL

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configurar autenticación JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    Console.WriteLine("JWT configurado correctamente");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($" Token inválido: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine(" Token válido.");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHttpClient<DoiService>();
builder.Services.AddHttpClient<PdfService>();
builder.Services.AddHttpClient<ZenodoService>();
builder.Services.AddHttpClient<ZenodoV2Service>();



builder.Services.AddControllers();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AkanjiApp", Version = "v1" });

    // Esto es necesario para que Swagger entienda los formularios con archivos

    // Añadir definición de seguridad
   c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce tu JWT con el formato: Bearer {token}"
    });

    // Aplicar la seguridad a todos los endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
    c.OperationFilter<FileUploadOperationFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

WaitForDatabase(builder.Configuration.GetConnectionString("DefaultConnection"), maxRetries: 20, delaySeconds: 5);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("PermitirFrontend");


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();

// Método auxiliar para esperar la base de datos
void WaitForDatabase(string connectionString, int maxRetries, int delaySeconds)
{
    int retry = 0;
    while (retry < maxRetries)
    {
        try
        {
            Console.WriteLine($"Intentando conectar a la base de datos... intento {retry + 1}/{maxRetries}");
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("¡Conexión a la base de datos exitosa!");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo conectar: {ex.Message}");
            retry++;
            if (retry >= maxRetries)
            {
                Console.WriteLine("Se alcanzó el número máximo de intentos. Abortando...");
                throw;
            }
            Thread.Sleep(delaySeconds * 1000);
        }
    }
}