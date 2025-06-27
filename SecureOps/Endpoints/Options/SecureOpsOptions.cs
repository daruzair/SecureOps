using Microsoft.AspNetCore.Routing;

namespace SecureOps.Endpoints.Options;

public class SecureOpsOptions
{

    public SecureOpsApiOptions? ApiOptions { get; private set; }

    /// <summary>
    /// Configures and prepares the SecureOps permission API endpoints.
    /// These will be registered by the main UseSecureOps() method.
    /// </summary>
    public void MapPermissionEndpoints(Action<SecureOpsApiOptions>? configure = null)
    {
        ApiOptions = new SecureOpsApiOptions();
        configure?.Invoke(ApiOptions);
    }
}

