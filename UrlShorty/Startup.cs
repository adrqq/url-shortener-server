using Microsoft.EntityFrameworkCore;
using Serilog;
using UrlShorty.Data;
using UrlShorty.Services;

namespace UrlShorty.StartupNs
{
    public class Startup(IConfiguration configuration)
    {
        private const string AllowReactAppPolicy = "AllowReactApp";

        public IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(AllowReactAppPolicy,
                    builder => builder
                        .WithOrigins("http://localhost:3000") // Update with your React app's URL
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton(Configuration);
            services.AddScoped<AccountService>();
            services.AddScoped<TokenService>();
            services.AddScoped<MailService>();
            services.AddScoped<ShortUrlService>();

            services.AddControllers();
            services.AddControllersWithViews();

            services.AddLogging(builder =>
            {
                builder.AddConsole(); // Example: Use console logging
                                      // Add other logging providers as needed
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/Home/Error");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(AllowReactAppPolicy);
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            /*app.UseAuthorization();*/

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");

                endpoints.MapControllerRoute(
                    name: "defaultView",
                    pattern: "",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}

