# Funcionamiento del login - Minimarket Jade

Este documento describe cómo está implementado el inicio y cierre de sesión en la aplicación Blazor Server.

---

## Resumen

- El login usa la tabla **Usuario** de la base de datos (SQL Server).
- La contraseña se guarda **hasheada** (SHA256 + Base64); nunca en texto plano.
- El estado “usuario logueado” se mantiene **en memoria** en un singleton (`AuthService`). No hay cookies ni JWT: si se reinicia la aplicación o se cierra el navegador, se pierde la sesión.
- Las páginas principales están protegidas: si el usuario no está autenticado, se redirige a `/login`.

---

## Flujo

1. **Entrar a la aplicación**  
   Si no hay sesión, `MainLayout` redirige a `/login`.

2. **Iniciar sesión** (`/login`)  
   El usuario ingresa nombre de usuario y contraseña. Se busca en la BD un usuario activo con ese nombre, se verifica el hash de la contraseña y, si es correcto, se llama a `AuthService.SignIn(user)` y se redirige a la página principal (`/`) con recarga completa.

3. **Navegación**  
   Con sesión activa, se muestra el menú lateral (Inicio, Ventas, Productos, etc.) y un enlace “Cerrar sesión”.

4. **Cerrar sesión** (`/logout`)  
   Se llama a `AuthService.SignOut()` y se redirige al navegador a `/login` mediante JavaScript para limpiar el estado de Blazor.

---

## Archivos y componentes

| Archivo | Descripción |
|--------|-------------|
| **Data/Entities/Usuario.cs** | Entidad: `IdUsuario`, `NombreUsuario`, `ContraseñaHash`, `Rol` (string), `Activo`. |
| **Data/AppDbContext.cs** | `DbSet<Usuario>` y mapeo a la tabla `Usuario` (columnas según script SQL). |
| **Services/PasswordHelper.cs** | `Hash(password)` y `Verify(password, storedHash)` con SHA256 + Base64. |
| **Services/AuthService.cs** | Singleton: `CurrentUser`, `IsAuthenticated`, `RolNombre`, `IsAdministrador`, `IsVendedor`, `SignIn(user)`, `SignOut()`. |
| **Components/Pages/Login.razor** | Página `/login`: formulario, consulta a BD, verificación de contraseña, `SignIn` y redirección. |
| **Components/Pages/Logout.razor** | Página `/logout`: `SignOut()` y redirección por JS a `/login`. |
| **Components/Layout/MainLayout.razor** | Si no está autenticado, redirige a `/login` y no muestra contenido. |
| **Components/Layout/LoginLayout.razor** | Layout mínimo para login y logout (sin menú). |
| **Components/Layout/LoginLayout.razor.css** | Estilos del layout de login (fondo, tarjeta centrada). |
| **Components/Layout/NavMenu.razor** | Menú condicional: con sesión muestra enlaces y “Cerrar sesión”; sin sesión, “Iniciar sesión”. |
| **Program.cs** | Registro de `AuthService` como singleton y lógica de arranque para el usuario `admin`. |

---

## Usuario admin al iniciar

Cada vez que la aplicación arranca, en `Program.cs` se ejecuta:

- Si **no existe** un usuario con nombre `admin`: se crea con contraseña hasheada de `admin123`, rol `Administrador` y `Activo = true`.
- Si **ya existe** el usuario `admin`: se actualiza su contraseña al hash de `admin123` y se asegura que esté activo.

**Credenciales por defecto:** usuario `admin`, contraseña `admin123`.

Así se puede entrar siempre con ese usuario tras ejecutar la app, tanto si la tabla viene del script `02_SeedData.sql` como si se creó desde la aplicación.

---

## Roles

El rol está en la columna `rol` de la tabla `Usuario` como texto. Los valores esperados son:

- `"Administrador"`
- `"Vendedor"`

En código se usan las propiedades de `AuthService`:

- `Auth.IsAdministrador` — rol igual a "Administrador" (sin distinguir mayúsculas).
- `Auth.IsVendedor` — rol igual a "Vendedor".

Para restringir una página o un ítem del menú por rol, se puede usar por ejemplo:

```razor
@if (Auth.IsAdministrador)
{
    <NavLink href="reportes">Reportes</NavLink>
}
```

---

## Seguridad (notas)

- **Sesión en memoria:** al reiniciar el servidor o abrir en otra pestaña/ventana sin sesión, hay que volver a iniciar sesión.
- **Hash:** se usa SHA256 + Base64. Para entornos más exigentes se recomienda BCrypt o Argon2.
- **HTTPS:** en producción conviene usar HTTPS para no enviar la contraseña en claro por la red (el formulario se envía por SignalR en Blazor Server, pero la configuración de HTTPS sigue siendo importante).

---

## Base de datos

La tabla `Usuario` debe existir y tener al menos las columnas: `id_usuario`, `nombre_usuario`, `contraseña_hash`, `rol`, `activo`. Los scripts en la carpeta `Scripts` del proyecto (por ejemplo `01_CreateTables.sql`) definen esta tabla.
