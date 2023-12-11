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
using PrintIt.Core.Pdfium;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using PrintIt.WebHost.RequestConsumers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.Text;

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
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Bearer token",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                      new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            },
                        },
                        new List<string>()
                    }
                });

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PrintIt API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddControllers();

            ConfigureJwtServices(services);


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

            if (AppSettings.JwtTokenKey != null) {
                app.UseAuthentication();

                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints => {
                if (AppSettings.JwtTokenKey == null) {
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

        private void ConfigureJwtServices(IServiceCollection services) {

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => {
                        options.Events = new JwtBearerEvents {
                            OnAuthenticationFailed = context => {
                                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException)) {
                                    context.Response.Headers.Add("Token-Expired", "true");
                                }
                                return Task.CompletedTask;
                            }
                        };
                        options.MapInboundClaims = true;
                        options.TokenValidationParameters = new TokenValidationParameters {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "https://ams.agmotion.com",
                            ValidAudience = "https://ams.agmotion.com",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.JwtTokenKey))
                        };
                    });
        }
    }
}
