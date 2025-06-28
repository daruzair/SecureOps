namespace SecureOps.Endpoints.Options;
/// <summary>
/// Provides configuration options for the SecureOps middleware, including settings for permission API endpoints.
/// </summary>
/// <remarks>This class is used to configure the SecureOps middleware, which handles permission-related API
/// endpoints. Use <see cref="MapPermissionEndpoints(Action{SecureOpsEndpointsOptions}?)"/> to define and customize the
/// endpoints.</remarks>
public class SecureOpsMiddlewareOptions
{
    /// <summary>
    /// Gets the configuration options for secure API endpoints.
    /// </summary>
    public SecureOpsEndpointsOptions? SecureOpsEndpointsOptions { get; private set; }

    /// <summary>
    /// Configures and prepares the SecureOps permission API endpoints.
    /// These will be registered by the main UseSecureOps() method.
    /// </summary>
    public void MapPermissionEndpoints(Action<SecureOpsEndpointsOptions>? configure = null)
    {
        SecureOpsEndpointsOptions = new SecureOpsEndpointsOptions();
        configure?.Invoke(SecureOpsEndpointsOptions);
    }
}

