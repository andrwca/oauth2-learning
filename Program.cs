using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using WebApiAuth.Authorization;
using WebApiAuth.Data;

namespace WebApiAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // For instance, 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles' claim.
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Add services to the container.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                {
                    builder.Configuration.Bind("AzureAd", options);
                    options.TokenValidationParameters.ValidateAudience = true;
                    options.TokenValidationParameters.ValidAudience = builder.Configuration["AzureAd:Audience"];
                }, options => { builder.Configuration.Bind("AzureAd", options); });

            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("ProjectOwner", p => p.Requirements.Add(new ProjectOwnerRequirement()));
            });

            builder.Services.AddSingleton<IAuthorizationHandler, ProjectOwnerHandler>();
            builder.Services.AddSingleton<IProjectsDatabase, ProjectDatabase>();
            builder.Services.AddControllers();

            var scopes = new string[] { "Project.Manage", "Project.View" };

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApiAuth", Version = "v1" });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,

                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(builder.Configuration["AzureAd:AuthUrl"] ?? throw new Exception("Configuration doesn't contain auth url.")),
                            TokenUrl = new Uri(builder.Configuration["AzureAd:TokenUrl"] ?? throw new Exception("Configuration doesn't contain token url.")),
                            Scopes = scopes.ToDictionary(s => $"{builder.Configuration["AzureAd:Audience"]}/{s}", s => s)
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        scopes.Select(s => $"{builder.Configuration["AzureAd:Audience"]}/{s}").ToArray()
                    }
                });
            });

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
                    c.OAuthUsePkce();
                });
            }

            // Use controllers
            app.MapControllers();

            app.Run();
        }
    }
}
