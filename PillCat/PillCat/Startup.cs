using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PillCat.Constants;
using PillCat.Facades.Extensions;
using PillCat.Middleware;
using PillCat.Models;

namespace PillCat
{
    ///<Summary>
    /// Startup
    ///</Summary>
    public class Startup
    {
        private const string SWAGGERFILE_PATH = "./swagger/v1/swagger.json";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Application configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingletons(Configuration);

            services.AddDbContext<PillContext>(
            o => o.UseInMemoryDatabase("PillCat"));

            services.AddDbContext<UserContext>(
            o => o.UseInMemoryDatabase("PillCat"));

            services.AddControllers();
            services.AddHealthChecks();
        }

        /// <summary>
        /// Configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.RoutePrefix = string.Empty;
                   c.SwaggerEndpoint(SWAGGERFILE_PATH, ProjectConstants.PROJECT_NAME + ProjectConstants.API_VERSION);
               });

            app.UseRouting()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapControllers();
               });

            app.UseStaticFiles(new StaticFileOptions
             {
                 FileProvider = new PhysicalFileProvider(
                 Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\cacag\\OneDrive\\Área de Trabalho\\Camila\\1. PUC\\6 Semestre\\TI - VI\\PillCat\\PillCat\\PillCat.Models\\Images\\")),
                 RequestPath = "/images"
             });
        }
    }
}