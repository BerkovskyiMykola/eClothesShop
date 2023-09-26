using Duende.IdentityServer.Models;

namespace Identity.API;

public static class IdentitServerConfig
{
    public static IEnumerable<IdentityResource> GetResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };
    }

    public static IEnumerable<ApiResource> GetApis()
    {
        return new List<ApiResource>
        {
            
        };
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            
        };
    }

    public static IEnumerable<Client> GetClients(IConfiguration configuration)
    {
        return new List<Client>
        {
            
        };
    }
}
