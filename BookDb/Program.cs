using BookDb.ExtendMethos;
using BookDb.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System;

namespace BookDb
{
    public class Program
    {
        public static void Main(string[] args)
        {


            CreateHostBuilder(args).Build().Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
                    Host.CreateDefaultBuilder(args)
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                        });

        public class Startup
        {
            public static string ContentRootPath { get; set; }
            public IConfiguration Configuration { get; }
            public Startup(IConfiguration configuration, IWebHostEnvironment env)
            {
                Configuration = configuration;
                ContentRootPath = env.ContentRootPath;
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddDbContext<AppDbContext>(option =>
                {
                    string connectString = Configuration.GetConnectionString("DefaultConnection");
                    option.UseSqlServer(connectString);
                });


                services.AddSingleton<FileStorageService>();

                services.AddControllersWithViews();
                services.AddRazorPages();

                services.Configure<RazorViewEngineOptions>(options =>
                {
                    options.ViewLocationFormats.Add("/MyViews/{1}/{0}" + RazorViewEngine.ViewExtension);
                });

                //services.Addsingleton<IHttpContextAccessor, HttpContextAccessor>();
            }
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/home/Error");
                    app.UseHsts();
                }
                app.UseHttpsRedirection();
                app.UseStaticFiles();


                app.AddStatusCodePage(); //tùy biến lỗi thừ 400 - 599

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();



                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();


                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                          name: "areas",
                          pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                        );
                    });

                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");

                    endpoints.MapRazorPages();
                });
            }
        }


    }
}
