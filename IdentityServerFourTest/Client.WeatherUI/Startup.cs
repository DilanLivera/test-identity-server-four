using Client.WeatherUI.Configurations;
using Client.WeatherUI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Client.WeatherUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddHttpClient();

            services.Configure<IdentityServerConfiguration>(
                Configuration.GetSection(nameof(IdentityServerConfiguration)));

            services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = "cookie";
                        options.DefaultChallengeScheme = "oidc";
                    })
                    .AddCookie("cookie")
                    .AddOpenIdConnect("oidc", options =>
                    {
                        options.Authority = Configuration["InteractiveServiceConfiguration:AuthorityUrl"];
                        options.ClientId = Configuration["InteractiveServiceConfiguration:ClientId"];
                        options.ClientSecret = Configuration["InteractiveServiceConfiguration:ClientSecret"];

                        options.ResponseType = "code";
                        options.UsePkce = true;
                        options.ResponseMode = "query";

                        options.Scope.Add(Configuration["InteractiveServiceSettings:Scopes:0"]);
                        options.SaveTokens = true;
                    });
             
            services.AddSingleton<IIdentityServerService, IdentityServerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

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
    }
}
