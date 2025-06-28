# 🔐 SecureOps — Feature-Based Permission Authorization for ASP.NET Core

**SecureOps** is a lightweight, extensible, and developer-friendly NuGet package for implementing feature-based (permission-based) authorization in ASP.NET Core. It provides attribute-level permission checks, customizable claim resolution, caching (in-memory or Redis), and optional API endpoints to manage permissions dynamically.

## 🚀 Features

- ✅ `[HasPermission("CanInsert")]` attribute support  
- ✅ Custom claim type for user identification (`sub`, `email`, etc.)  
- ✅ In-memory or Redis caching (plug-and-play)  
- ✅ Configurable authentication scheme support  
- ✅ Optional API endpoints to manage permissions  
- ✅ Minimal API compatible  
- ✅ Clean and extensible design  

## 📦 Installation

> Coming soon to NuGet:

```bash
dotnet add package SecureOps
```

Or clone and build from source:

```bash
git clone https://github.com/eruzairshafi/SecureOps.git
cd SecureOps
dotnet build
```

## 🔧 Usage

### 1. Register SecureOps in your `Program.cs`

```csharp
builder.Services.AddSecureOps(options =>
{
    options.UserIdClaimType = "sub"; // Or "email", etc.
    options.CacheMode = CacheMode.Memory; // Or CacheMode.Redis
    options.AuthenticationOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});
```

Or with a custom store:

```csharp
builder.Services.AddSecureOps<CustomDbPermissionStore>(options => { ... });
```

### 2. Enable middleware

```csharp
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseSecureOps(config =>
    config.MapPermissionEndpoints(api =>
    {
        api.RoutePrefix = "api/permissions";
        api.PermissionClaim = "ManagePermissions"; // Optional: claim required to access the endpoints
        api.EnableUserPermissionManagement = true;
    }));

app.MapControllers();
app.Run();
```

### 3. Use `[HasPermission]` in your controllers

```csharp
[HasPermission("CanInsert")]
[HttpPost]
public IActionResult CreateItem() => Ok("Item created.");
```

Define feature-level permissions like:
- `CanInsert`
- `Product.View`
- `Order.Delete`

These permissions are stored and checked per user.

## ⚙️ Configuration Options

```csharp
builder.Services.AddSecureOps(options =>
{
    options.UserIdClaimType = "sub"; // Claim to extract user ID from token
    options.CacheMode = CacheMode.Redis;
    options.AuthenticationOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});
```

```csharp
app.UseSecureOps(config =>
{
    config.MapPermissionEndpoints(api =>
    {
        api.RoutePrefix = "api/permissions";
        api.PermissionClaim = "ManagePermissions"; // null = no auth required
        api.EnableUserPermissionManagement = true;
        api.EnableGlobalPermissionManagement = false;
        api.EnableListingAllPermissions = true;
    });
});
```

## 🔐 Claim-Based Access Control

SecureOps lets you configure which claim is used to extract the user identity:

```csharp
options.UserIdClaimType = "sub"; // Common in JWT tokens
```

## 🧠 Caching Strategy

Choose between:
- `CacheMode.Memory` (default)
- `CacheMode.Redis` — requires Redis connection string

Example:

```csharp
options.CacheMode = CacheMode.Redis;
options.RedisConfiguration = "localhost:6379";
```

## 🧪 Exposing Management Endpoints

SecureOps optionally exposes permission management APIs:

- `GET /api/permissions/users/{userId}`
- `POST /api/permissions/users/{userId}`
- `DELETE /api/permissions/users/{userId}`
- `GET /api/permissions/all`

Control what is exposed via:

```csharp
config.MapPermissionEndpoints(api =>
{
    api.EnableUserPermissionManagement = true;
    api.EnableGlobalPermissionManagement = false;
    api.EnableListingAllPermissions = true;
});
```

## 📁 Folder Structure

```
SecureOps/
├── Attributes/
│   └── HasPermissionAttribute.cs
├── Endpoints/
│   ├── SecureOpsAppBuilderExtensions.cs
│   ├── Options/
│   │   └── SecureOpsEndpointsOptions.cs
│   └── Model/
│       └── PermissionRequest.cs
├── Services/
│   ├── IPermissionService.cs
│   ├── InMemoryPermissionStore.cs
│   └── CachedPermissionService.cs
├── SecureOpsExtensions.cs
├── README.md
```

## 📝 License

This project is licensed under the MIT License. See `LICENSE.txt` for details.

## ❤️ Contributing

Feel free to open issues, submit PRs, or suggest features. Contributions are welcome!

## ✉️ Contact

Made with ❤️ by Uzair Shafi  
GitHub: eruzairshafi
