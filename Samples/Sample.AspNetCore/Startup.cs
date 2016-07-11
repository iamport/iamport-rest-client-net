﻿using Iamport.RestApi;
using Iamport.RestApi.Apis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.AspNetCore.Repositories;

namespace Sample.AspNetCore
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
            // 리포지터리 등록
            // 샘플이라 단순한 in-memory static 저장소를 사용합니다.
            services.AddSingleton<PaymentRepository>();

            // 아임포트 서비스 등록
            services.AddSingleton(serviceProvider =>
            {
                var options =
                    serviceProvider
                    .GetService<IOptions<IamportHttpClientOptions>>();
                return options.Value;
            });
            services.AddSingleton<IIamportClient, IamportHttpClient>();
            services.AddSingleton<IPaymentsApi, PaymentsApi>();
            services.Configure<IamportHttpClientOptions>(
                Configuration.GetSection("iamport"));

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}