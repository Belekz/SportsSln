using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SportsStore.Models;
using Microsoft.AspNetCore.Identity;

namespace SportsStore
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }
        private IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Views
            services.AddControllersWithViews();

            // Database repo
            services.AddDbContext<StoreDbContext>(opts => 
            { 
                opts.UseSqlServer(Configuration["ConnectionStrings:SportsStoreConnection"]);
            });
            services.AddScoped<IStoreRepository, EFStoreRepository>();
            services.AddScoped<IOrderRepository, EFOrderRepository>();

            // Razor Pages
            services.AddRazorPages();

            // Sessions 
            services.AddDistributedMemoryCache();
            services.AddSession();

            // Cart service
            services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Server side Blazor
            services.AddServerSideBlazor();

            // Identity Package for Entity Framework Core
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:IdentityConnection"]));
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Error handling
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();

            if (env.IsProduction())
            {
                app.UseExceptionHandler("/error");
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }

            // Static 
            app.UseStaticFiles();

            // session memory
            app.UseSession();

            // Routing
            app.UseRouting();

            // Identity (always write between routing & endpoints!) 
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute("catpage","{category}/Page{productPage:int}",new { Controller = "Home", action = "Index" });
                endpoints.MapControllerRoute("page", "Page{productPage:int}",new { Controller = "Home", action = "Index", productPage = 1 });
                endpoints.MapControllerRoute("category", "{category}",new { Controller = "Home", action = "Index", productPage = 1 });
                endpoints.MapControllerRoute("pagination","Products/Page{productPage}",new { Controller = "Home", action = "Index", productPage = 1 });

                endpoints.MapDefaultControllerRoute();

                // Razor Pages
                endpoints.MapRazorPages();

                // Server side Blazor
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/admin/{*catchall}", "/Admin/Index");
            });

            // Populate database with sample data & user management
            SeedData.EnsurePopulated(app);
            IdentitySeedData.EnsurePopulated(app);
        }
    }
}

