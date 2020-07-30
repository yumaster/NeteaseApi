using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CloudMusicDotNet.Api.Infrastructure;
using CloudMusicDotNet.Commons.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using yumaster.Util;

namespace CloudMusicDotNet.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment env)
        {
            Configuration = configuration;
            GlobalContext.LogWhenStart(env);
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "网易云音乐 API",
                    Version = "v1",
                    Description = "网易云音乐接口",
                    //作者信息
                    Contact = new OpenApiContact
                    {
                        Name = "土伦",
                        Email = string.Empty,
                        Url = new Uri("http://yumaster.net"),
                    },
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddHttpContextAccessor();

            services.AddTransient<HttpClientCookieDelegatingHandler>();
            services.AddHttpClient("CloudMusic")
                .AddHttpMessageHandler<HttpClientCookieDelegatingHandler>();

            services.AddTransient<IDtoParseService, DtoParseService>();

            GlobalContext.SystemConfig = Configuration.GetSection("SystemConfig").Get<SystemConfig>();

            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder.RegisterAssemblyTypes(typeof(IAlbumService).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();

            return new AutofacServiceProvider(builder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "网易云音乐 API V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
