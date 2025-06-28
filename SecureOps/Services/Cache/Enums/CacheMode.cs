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
    /// <summary>
    /// Represents a contiguous region of memory that can be used to store and manipulate data.
    /// </summary>
    /// <remarks>The <see cref="Memory{T}"/> structure provides a way to work with memory buffers in a
    /// type-safe manner. It is commonly used for scenarios involving slicing, copying, and passing memory regions
    /// without allocating new arrays or buffers. Unlike <see cref="Span{T}"/>, <see cref="Memory{T}"/> is not
    /// stack-only and can be used across asynchronous methods.</remarks>
    Memory,

    /// <summary>
    /// Represents a Redis client for interacting with a Redis database.
    /// </summary>
    /// <remarks>This class provides methods and properties for performing operations on a Redis database, 
    /// such as setting and retrieving key-value pairs, managing connections, and executing commands. It is designed to
    /// facilitate communication with Redis servers in a .NET application.</remarks>
    Redis
}

