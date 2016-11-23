using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nelibur.ObjectMapper;
using OpenGameListWebApp.Data;
using OpenGameListWebApp.Data.Items;
using OpenGameListWebApp.ViewModels;

namespace OpenGameListWebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddEntityFramework();

            services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"])
            );

            services.AddSingleton<DbSeeder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            DbSeeder dbSeeder)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    //Disable caching for all static files.
                    context.Context.Response.Headers["Cache-Control"] =
                        Configuration["StaticFiles:Headers:Cache-Control"];
                    context.Context.Response.Headers["Pragma"] = Configuration["StaticFiles:Headers:Pragma"];
                    context.Context.Response.Headers["Expires"] = Configuration["StaticFiles:Headers:Expires"];
                }
            });

            app.UseMvc();

            //TinyMapper binding configuration
            TinyMapper.Bind<Item, ItemViewModel>();

            try
            {
                dbSeeder.SeedAsync().Wait();
            }
            catch (AggregateException e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}
