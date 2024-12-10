using Microsoft.EntityFrameworkCore;
using DuootApi.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// Configure the connection to PostgreSQL
builder.Services.AddDbContext<DuootDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DuootDatabase")));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignorar referencias cíclicas
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add CORS policy to allow any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Permite solicitudes desde cualquier origen
              .AllowAnyHeader() // Permite cualquier encabezado
              .AllowAnyMethod(); // Permite cualquier método (GET, POST, PUT, DELETE, etc.)
    });
});

// Configure JWT authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Configure the authorization service
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Apply the "AllowAll" CORS policy
app.UseCors("AllowAll");

// Habilitar servir archivos estáticos (para las imágenes)
// Colocarlo antes de UseAuthentication y UseAuthorization para evitar restricciones en las imágenes
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
    RequestPath = "/Images"
});


// Middleware for authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configure Kestrel to listen on all network interfaces
var port = builder.Configuration.GetValue("PORT", 5000); // Default to port 5000 if no value provided
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
