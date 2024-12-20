using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrinterSystem.Database;
using PrinterSystem.Models;
using PrinterSystem.Services;
using System.Text;

namespace PrinterSystem
{
    public class Program
    {
        public static string ConnectionString { get; private set; }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Getting connection string
            ConnectionString = builder.Configuration.GetConnectionString("SQL");

            // Configure JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            //Setting up cors policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorClient", builder =>
                {
                    builder.WithOrigins("https://localhost:7126") // Blazor WebAssembly URL
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });

            // Configure Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole(nameof(Role.Admin)));

                options.AddPolicy("SeniorPolicy", policy =>
                    policy.RequireRole(nameof(Role.Senior), nameof(Role.Admin)));

                options.AddPolicy("KIBPolicy", policy =>
                    policy.RequireRole(nameof(Role.KIB), nameof(Role.Senior), nameof(Role.Admin)));
            });

            builder.Services.AddScoped<JWTService>();

            // Add services to the container
            builder.Services.AddControllers();

            // Swagger/OpenAPI setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowBlazorClient");
            // Enable Authentication and Authorization
            app.UseAuthentication(); // This middleware validates JWT tokens
            app.UseAuthorization();  // This middleware enforces authorization policies

            app.MapControllers();

            app.Run();
        }
    }
}
