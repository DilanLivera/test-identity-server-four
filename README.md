# Test IdentityServer 4

## IdentityServer project setup

- Add **[IdentityServer4](https://www.nuget.org/packages/IdentityServer4/)** package
- Configure IdentityServer4 in `Startup.cs` file
  - Register IdentityServer in DI container

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        // For more info about these extension methods goto https://identityserver4.readthedocs.io/en/latest/topics/startup.html#key-material
        services.AddIdentityServer()
                .AddInMemoryClients(new List<Client>())
                .AddInMemoryIdentityResources(new List<IdentityResource>())
                .AddInMemoryApiResources(new List<ApiResource>())
                .AddInMemoryApiScopes(new List<ApiScope>())
                .AddTestUsers(new List<TestUser>())
                .AddDeveloperSigningCredential();
    }
    ```

    Eg.

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddIdentityServer()
                .AddInMemoryClients(Config.Clients)
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddTestUsers(Config.Users) 
                .AddDeveloperSigningCredential();
    }
    ```

    ```csharp
    /*
     * Below code is generated when we use the is4inmem template(i.e. dotnet new is4inmem)
     * 👇 is copied from https://github.com/kevinrjones/SettingUpIdentityServer/blob/master/recordeddemo/identity/ids/Config.cs file
     */
    public static class Config
    {
        public static List<TestUser> Users
        {
            get
            {
                var address = new
                {
                    street_address = "One Hacker Way",
                    locality = "Heidelberg",
                    postal_code = 69118,
                    country = "Germany"
                };

                return new List<TestUser>
                {
                    new TestUser
                    {
                        SubjectId = "818727",
                        Username = "alice",
                        Password = "alice",
                        Claims =
                        {
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.Role, "admin"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                            new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
                        }
                    },
                    new TestUser
                    {
                        SubjectId = "88421113",
                        Username = "bob",
                        Password = "bob",
                        Claims =
                        {
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                            new Claim(JwtClaimTypes.Role, "user"),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                            new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json)
                        }
                    }
                };
            }
        }

        public static IEnumerable<IdentityResource> IdentityResources => new[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };

        public static IEnumerable<ApiScope> ApiScopes => new[]
        {
            new ApiScope("weatherapi.read"),
            new ApiScope("weatherapi.write")
        };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("weatherapi")
            {
                Scopes = new List<string> {"weatherapi.read", "weatherapi.write"},
                ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                UserClaims = new List<string> {"role"}
            }
        };

        public static IEnumerable<Client> Clients => new[]
        {
            // m2m(machine2machine) client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = {new Secret("SuperSecretPassword".Sha256())},

                AllowedScopes = {"weatherapi.read", "weatherapi.write"}
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("SuperSecretPassword".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = {"https://localhost:5444/signin-oidc"},
                FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
                PostLogoutRedirectUris = {"https://localhost:5444/signout-callback-oidc"},

                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile", "weatherapi.read"},
                RequirePkce = true,
                RequireConsent = true,
                AllowPlainTextPkce = false
            }
        };
    }
    ```

  - Add IdentityServer in to the middlewear pipeline

    ```csharp
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseIdentityServer();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
    ```

  - Ask IdentityServer for the OpenId connect discovery document by going to `{endPoint}/.well-known/openid-configuration`. Eg. `https://localhost:44369/.well-known/openid-configuration`

## Client API project setup

- Create a API project
- Add **[IdentityServer4.AccessTokenValidation](https://www.nuget.org/packages/IdentityServer4.AccessTokenValidation/)** package
- Register services required by the identity server in DI container

  ```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddAuthentication("Bearer")
              .AddIdentityServerAuthentication(
                  "Bearer", 
                  authenticationOptions =>
                  {
                      authenticationOptions.ApiName = "weatherapi";
                      authenticationOptions.Authority = "https://localhost:5001";
                  });
  }
  ```

- Add authentication middlwear

  ```csharp
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
          endpoints.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");
      });
  }
  ```

- Protect the controller by adding the `[Authorize]` attribute

  ```csharp
  [ApiController]
  [Route("[controller]")]
  [Authorize]
  public class WeatherForecastController : ControllerBase
  {
  }
  ```

## Client UI project setup

[comment]: <> (TODO : Add Client project setup)

## Credits

- [YouTube - Creating your First IdentityServer4 Solution](https://www.youtube.com/watch?v=HJQ2-sJURvA)
- [YouTube - Introduction to IdentityServer4 for ASP NET Core Part I](https://www.youtube.com/watch?v=rP8pI0BCUMY)
- [YouTube - Introduction to IdentityServer for ASP.NET Core Part 2](https://www.youtube.com/watch?v=7qJ4YS3Azd8&t=2575s)
- [YouTube - Introduction to IdentityServer for ASP.NET Core Part 3](https://www.youtube.com/watch?v=Jd7Dy8YObxo&t=43s)
- [YouTube - Introduction to IdentityServer for ASP.NET Core Part 4](https://www.youtube.com/watch?v=AX3vZyugvBQ&t=18s)

## Resources

- [IdentityServer4 Docs](https://identityserver4.readthedocs.io/en/latest/index.html)
- [IdentityServer4 Documentation](https://docs.identityserver.io/_/downloads/en/latest/pdf/)