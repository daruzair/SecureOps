namespace SecureOps.Services.Cache.Enums;
/// <summary>
/// Specifies the caching mode to be used for storing and retrieving data.
/// </summary>
/// <remarks>Use <see cref="CacheMode.Memory"/> for in-memory caching, which is suitable for scenarios where data
/// needs to be stored temporarily within the application's process. Use  <see cref="CacheMode.Redis"/> for distributed
/// caching, which is ideal for scenarios requiring shared caching across multiple application instances or
/// servers.</remarks>
public enum CacheMode
{
    Memory,
    Redis
}

