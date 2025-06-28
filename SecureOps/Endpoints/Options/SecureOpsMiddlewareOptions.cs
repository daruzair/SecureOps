namespace SecureOps.Endpoints.Options;

public class SecureOpsMiddlewareOptions
{

    public SecureOpsEndpointsOptions? ApiOptions { get; private set; }

    /// <summary>
    /// Configures and prepares the SecureOps permission API endpoints.
    /// These will be registered by the main UseSecureOps() method.
    /// </summary>
    public void MapPermissionEndpoints(Action<SecureOpsEndpointsOptions>? configure = null)
    {
        ApiOptions = new SecureOpsEndpointsOptions();
        configure?.Invoke(ApiOptions);
    }
}

