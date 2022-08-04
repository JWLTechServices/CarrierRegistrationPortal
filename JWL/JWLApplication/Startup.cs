using Data;
using Interfaces;
using JWLApplication.Filters;
using LoggerService.Contracts;
using LoggerService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using NLog;
using System;
using System.Globalization;
using System.IO;
using AutoMapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;

namespace JWLApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<ILoggerManager, LoggerManager>();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });
            services.Configure<RequestLocalizationOptions>(opts =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("en-IN"),
                };
                opts.DefaultRequestCulture = new RequestCulture("en-US");
                // Formatting numbers, dates, etc.
                opts.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                opts.SupportedUICultures = supportedCultures;
            });
            services.AddControllersWithViews();

            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddDbContext<JWLDBContext>(options =>
               options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
            services.AddHttpContextAccessor();
            services.AddSingleton<MyActionFilterAttribute>();
            services.AddScoped<ICarrierService, CarrierService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<IMarkeSstateService, MarkeSstateService>();
            services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            services.AddScoped<ICargoService, CargoService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ICarrierProcessingService, CarrierProcessingService>();
            services.AddScoped<ITrailerService, TrailerService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<RecaptchaSettings>(Configuration.GetSection("RecaptchaSettings"));
            services.AddScoped<GoogleRecaptchaService>();
            services.Configure<DatatracSettings>(Configuration.GetSection("DatatracSettings"));
            services.AddScoped<DatatracService>();
            services.Configure<DynamicsSettings>(Configuration.GetSection("DynamicsSettings"));
            services.AddScoped<DynamicsService>();
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
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    var newPath = new PathString("/Error/404");
                    var originalPath = context.Request.Path;
                    var originalQueryString = context.Request.QueryString;
                    context.Features.Set<IStatusCodeReExecuteFeature>(new StatusCodeReExecuteFeature()
                    {
                        OriginalPathBase = context.Request.PathBase.Value,
                        OriginalPath = originalPath.Value,
                        OriginalQueryString = originalQueryString.HasValue ? originalQueryString.Value : null
                    });
                    context.SetEndpoint(endpoint: null);
                    var routeValueFeature = context.Features.Get<IRouteValuesFeature>();
                    routeValueFeature?.RouteValues?.Clear();
                    context.Request.Path = newPath;
                    try
                    {
                        await next();
                    }
                    finally
                    {
                        context.Request.QueryString = originalQueryString;
                        context.Request.Path = originalPath;
                        context.Features.Set<IStatusCodePagesFeature>(null);
                    }
                }
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRequestLocalization();
            app.UseRouting();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthorization();
            app.UseAuthorization();


            app.UseCustomRoute();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
