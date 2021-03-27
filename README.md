# Test IdentityServer 4

A test project to practice and test IdentityServer 4

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

  - 

## Resources

- [IdentityServer4 Docs](https://identityserver4.readthedocs.io/en/latest/index.html)
