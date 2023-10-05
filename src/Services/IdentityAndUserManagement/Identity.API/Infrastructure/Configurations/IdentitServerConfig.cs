using Duende.IdentityServer.Models;

namespace Identity.API.Infrastructure.Configurations;

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
            new ApiResource {
                Name = "user-management",
                DisplayName = "User Management Service",
                Scopes = new List<string> {
                    "user-management"
                }
            },
        };
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope("user-management") {  },
        };
    }

    public static IEnumerable<Client> GetClients(IConfiguration configuration)
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = "user-management-swagger-ui",
                ClientName = "User Management Swagger UI",
                RequireClientSecret = false,
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"{configuration["UserManagementApiExternal"]}/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins = { $"{configuration["UserManagementApiExternal"]}" },
                AllowedScopes = { "openid", "profile", "user-management" },
            },
        };
    }
}
