using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrintIt.Core;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using System.IO;
using Serilog;
using PrintIt.Core.Pdfium;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using PrintIt.WebHost.RequestConsumers;

namespace PrintIt.WebHost {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public AppSettings AppSettings { get; set; }

        public string CorsPolicy => "CORS";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            var appSettings = new AppSettings();
            Configuration.Bind(appSettings);
            AppSettings = appSettings;

            services.AddOptions<AppSettings>()
                    .Bind(Configuration)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            PdfLibrary.EnsureInitialized();

            services.AddPrintIt();

            services.AddCors(options => {
                options.AddPolicy(name: CorsPolicy, builder => {
                    builder
                    .WithOrigins(AppSettings.AllowedCors)
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
                });
            });

            if (AppSettings.RabbitMqSettings != null) {
                services.AddMassTransit(x => {
                    x.UsingRabbitMq((ctx, cfg) => {
                        cfg.Host(new Uri(appSettings.RabbitMqSettings.Host), host => {
                            host.Username(AppSettings.RabbitMqSettings.Username);
                            host.Password(AppSettings.RabbitMqSettings.Password);
                        });
                        cfg.ConfigureEndpoints(ctx);
                    });
                    x.AddConsumer<PrintConsumer>();
                });
                services.AddMassTransitHostedService();
            }

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PrintIt API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            if (AppSettings.DataProtection != null) {
                services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(AppSettings.DataProtection.PersistKeyPath))
                .SetApplicationName(AppSettings.DataProtection.ApplicationName);
            }

            if (AppSettings.CookieAuth != null) {
                services.AddAuthentication("ApplicationCookie")
                               .AddCookie("ApplicationCookie", options => {
                                   options.Cookie.Path = "/";
                                   options.Cookie.Name = AppSettings.CookieAuth.AppCookieName;
                               });
            }

            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


#if DEBUG
            app.UseSerilogRequestLogging();
#endif

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(CorsPolicy);

            if (AppSettings.CookieAuth != null) {
                app.UseAuthentication();

                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints => {
                if (AppSettings.CookieAuth == null) {
                    endpoints.MapControllers();
                } else {
                    endpoints.MapControllers().RequireAuthorization(new AuthorizeAttribute());
                }
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PrintIt API V1");
                c.RoutePrefix = string.Empty;
            });

        }
    }
}
