using AuthServer.Extensions;
using AuthServer.Infrastructure.Data.Identity;
using AuthServer.Infrastructure.Services;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthServer
{
    public static class InjectorBootStrapper
    {
        public static void AddLayersInjector(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_AUTHSERVER");
            services.AddDbContext<AppIdentityDbContext>(options => options.UseMySQL(connectionString));

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();
            //.AddTokenProvider<CreateStaffTokenProvider<AppUser>>("CreateStaff");

            services.AddIdentityServer(options =>
                {
                    options.Authentication.CookieLifetime = TimeSpan.FromDays(30);
                    options.Authentication.CookieSlidingExpiration = true;
                })
                .AddDeveloperSigningCredential()
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseMySQL(connectionString);
                    // this enables automatic token cleanup. this is optional.
                    //options.EnableTokenCleanup = true;
                    //options.TokenCleanupInterval = 30; // interval in seconds
                })
                //.AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients(configuration))
                .AddAspNetIdentity<AppUser>();

            services.AddSameSiteCookiePolicy();

            services.AddTransient<IProfileService, IdentityClaimsProfileService>();

            // Client services
            services.AddHttpClient<IStaffClientService>();
            services.AddTransient<IStaffClientService, StaffClientService>();

        }
    }
}
