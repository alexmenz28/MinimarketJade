using MinimarketJade.Web.Components;
using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Services.Auth;
using MinimarketJade.Web.Services.Categorias;
using MinimarketJade.Web.Services.Clientes;
using MinimarketJade.Web.Services.Productos;
using MinimarketJade.Web.Services.Proveedores;

var builder = WebApplication.CreateBuilder(args);

// Base de datos (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Servicios de aplicación
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();

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


