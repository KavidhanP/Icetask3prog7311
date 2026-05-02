using LogiTech.Data;
using LogiTech.Services;
using Microsoft.EntityFrameworkCore;

namespace LogiTech
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<LogiTechDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<RouteService>();
            builder.Services.AddControllersWithViews();

            
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();       
            app.UseSession();        
            app.UseAuthorization();

            app.MapControllerRoute(
     name: "default",
     pattern: "{controller=Shipment}/{action=Index}/{id?}");

            app.Run();
        }
    }
}