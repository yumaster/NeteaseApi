﻿using System;
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
            services.AddQueuePolicy(options =>
            {
                //最大并发请求数
                options.MaxConcurrentRequests = 2;
                //请求队列长度限制
                options.RequestQueueLimit = 1000;
            });

            services.AddCors(option => option.AddPolicy("AllowAll", policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(new []{ "https://music.163.com" })));
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

            //services.AddHttpClient("CloudMusic")
            //    .AddHttpMessageHandler<HttpClientCookieDelegatingHandler>();

            services.AddHttpClient();
            services.AddHttpClient("CloudMusic", c =>
            {
                //c.BaseAddress = new Uri("https://music.163.com");
                c.DefaultRequestHeaders.Add("Referer", "https://music.163.com");
            });

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
            //添加并发限制中间件
            app.UseConcurrencyLimiter();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("AllowAll");
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
