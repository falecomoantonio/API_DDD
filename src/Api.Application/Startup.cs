using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CrossCutting.DepedencyInjection;
using Domain.Security;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using Microsoft.AspNetCore.Authorization;

namespace application
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureService.ConfigureDependenciesService(services);
            ConfigureRepository.ConfigureDependenciesRepository(services);

            var signingConfiguration = new SigninConfiguration();
            services.AddSingleton(signingConfiguration);

            var tokenConfigurations = new TokenConfiguration();
            new ConfigureFromConfigurationOptions<TokenConfiguration>(Configuration.GetSection("TokenConfigurations")).Configure(tokenConfigurations);

            services.AddSingleton(tokenConfigurations);

            services.AddControllers();
            services.AddSwaggerGen(x => {
                x.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "1.0.0.0",
                    Title = "MyApi Vers�o 1",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Email = "falecomoantonio@live.com",
                        Name = "Antonio Jos�",
                        Url = new System.Uri("http://suporte.updev.net.br")
                    },
                    Description = "API de Aprendizagem com arquitetura DDD",
                    License = new Microsoft.OpenApi.Models.OpenApiLicense
                    {
                        Url = new System.Uri("http://suporte.updev.net.br/licence"),
                        Name = "Termos de uso"
                    },
                    TermsOfService = new System.Uri("http://suporte.updev.net.br/licence")
                }); 
            });


            services.AddAuthentication(option =>
                    {
                        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options => {
                        var paramsOptions = options.TokenValidationParameters;
                        paramsOptions.IssuerSigningKey = signingConfiguration.Key;
                        paramsOptions.ValidAudience = tokenConfigurations.Audience;
                        paramsOptions.ValidIssuer = tokenConfigurations.Issuer;
                        paramsOptions.ValidateIssuerSigningKey = true;
                        paramsOptions.ValidateLifetime = true;
                        paramsOptions.ClockSkew = TimeSpan.Zero;

                    });

            services.AddAuthorization(auth => {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json","MyApi");
                x.RoutePrefix = string.Empty;
            });

            app.UseRouting();


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
