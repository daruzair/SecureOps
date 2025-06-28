using Microsoft.AspNetCore.Authentication;
using SecureOps.Services.Cache.Enums;
using System.Security.Claims;

namespace SecureOps.Services.Cache.Options;

public class SecureOpsOptions
{
    public CacheMode CacheMode { get; set; } = CacheMode.Memory;
    public AuthenticationOptions AuthenticationOptions { get; set; } = new();
    public string UserIdClaimType { get; set; } = ClaimTypes.Name;
    public void UseRedis() => CacheMode = CacheMode.Redis;
    public void UseMemory() => CacheMode = CacheMode.Memory;
    
}

