// =========================
// 🐙 GitHub README (for GitHub repo)
// =========================

# SecureOps 🔐

[![NuGet Version](https://img.shields.io/nuget/v/SecureOps.svg)](https://www.nuget.org/packages/SecureOps)
[![CI/CD](https://github.com/eruzairshafi/SecureOps/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/eruzairshafi/SecureOps/actions)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**SecureOps** is an open-source feature/permission-based authorization system for ASP.NET Core.

It enables:
- Clean permission-based endpoint protection (like `"Product.View"`)
- Caching with Redis or Memory
- Configurable user claim resolution (`sub`, `email`, `name`, etc.)
- Optional permission management endpoints (minimal API)
- Extensibility with custom stores (DB, file, in-memory)

---

## Installation

```bash
dotnet add package SecureOps
```

---

## Quick Start

### Program.cs

```csharp
builder.Services.AddSecureOps(options =>
{
    options.CacheMode = CacheMode.Memory;
    options.UserIdClaimType = "sub";
    options.AuthenticationOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});
```

### Protect Controller Actions

```csharp
[HasPermission("Invoice.View")]
[HttpGet("invoices")]
public IActionResult GetInvoices() => Ok("Allowed");
```

---

## Enable Permission Management API (Optional)

```csharp
app.UseSecureOps(secure =>
{
    secure.MapPermissionEndpoints(api =>
    {
        api.RoutePrefix = "api/permissions";
        api.PermissionClaim = "ManagePermissions";
        api.EnableUserPermissionManagement = true;
    });
});
```

- Protect these endpoints with your desired permission claim
- Easily integrate with your own UI for permission administration

---

## Configuration

| Option                     | Description                                      | Default        |
|----------------------------|--------------------------------------------------|----------------|
| `CacheMode`                | `Memory` or `Redis`                              | `Memory`       |
| `RedisConfiguration`       | Used only if Redis is enabled                    | `null`         |
| `UserIdClaimType`          | Claim type to extract user ID (`sub`, `email`)   | `name`         |
| `PermissionClaim`          | Required claim to access permission API          | `null` (open)  |

---

## Example

Check out the included `SecureOps.Example` project for a working implementation.

---

## License

Licensed under the [MIT License](LICENSE).

---

## 🙋‍♂️ Contributions

Contributions, issues, and ideas welcome! Open a PR or raise an issue 🚀