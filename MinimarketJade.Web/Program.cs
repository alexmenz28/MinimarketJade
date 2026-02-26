using MinimarketJade.Web.Components;
using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Base de datos (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicio de categorías: el frontend lo inyecta (ICategoriaService) para listar y gestionar la jerarquía de categorías.
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// Autenticación en memoria (login/logout): estado del usuario actual.
builder.Services.AddSingleton<AuthService>();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Al iniciar: asegurar usuario admin con contraseña admin123 (crear si no existe, o actualizar hash si ya existe).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var adminHash = PasswordHelper.Hash("admin123");
    var admin = db.Usuarios.FirstOrDefault(u => u.NombreUsuario == "admin");
    if (admin == null)
    {
        db.Usuarios.Add(new Usuario
        {
            NombreUsuario = "admin",
            ContraseñaHash = adminHash,
            Rol = "Administrador",
            Activo = true
        });
    }
    else
    {
        admin.ContraseñaHash = adminHash;
        admin.Activo = true;
    }
    db.SaveChanges();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
