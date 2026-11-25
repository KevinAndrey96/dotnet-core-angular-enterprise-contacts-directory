using System;
using System.Text;
using API.Middleware;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. CORS Policy
            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // 2. JWT Bearer Authentication
            string secretKey = Configuration.GetValue<string>("SecretKey") ?? "DefaultSuperSecretKeyForContactsDirectory2026";
            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // 3. Database Provider (SQLite default for universal portability, or SQL Server)
            string provider = Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
            string connectionString = Configuration.GetConnectionString("DefaultConnection") 
                ?? Configuration.GetConnectionString("SqliteConnection") 
                ?? "Data Source=directory.db";

            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<UserContext>(opt => opt.UseSqlServer(connectionString));
            }
            else
            {
                services.AddDbContext<UserContext>(opt => opt.UseSqlite(connectionString));
            }

            // 4. Domain Services Dependency Injection
            services.AddSingleton<IMongoLogService, MongoLogService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IContactService, ContactService>();

            // 5. Controllers
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // 6. Swagger / OpenAPI Documentation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Contacts Directory Enterprise API",
                    Version = "v1",
                    Description = "RESTful API built with ASP.NET Core, EF Core, SQL Server/SQLite, and MongoDB Exception Logging. Developed for Hubbec Fullstack Challenge.",
                    Contact = new OpenApiContact
                    {
                        Name = "Kevin Andrey Herrera Silva",
                        Email = "kahs_kevin@hotmail.com",
                        Url = new Uri("https://github.com/KevinAndrey96")
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserContext dbContext)
        {
            // Auto-create database schema and seed sample demo data
            try
            {
                dbContext.Database.EnsureCreated();
                dbContext.SeedInitialData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization notice: {ex.Message}");
            }

            app.UseCors("AllowAll");

            // Global Exception Interception Middleware (Logs to MongoDB)
            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (env.IsDevelopment() || true)
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contacts Directory API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    context.Response.Redirect("/swagger");
                });
            });
        }
    }
}
