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
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,

                RedirectUris = { $"{configuration["UserManagementApi"]}/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{configuration["UserManagementApi"]}/swagger/" },

                AllowedScopes = { "user-management" },
            },
        };
    }
}
