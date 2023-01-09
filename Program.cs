using AuthServer;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Net;

const string CORS_NAME = "AllowAll";
IConfiguration configuration;

var builder = WebApplication.CreateBuilder(args);

var configurationBuilder = new ConfigurationBuilder().AddCommandLine(args)
    .SetBasePath(Directory.GetCurrentDirectory());

// system env
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_AUTHSERVER");

builder.Environment.EnvironmentName = env;

configurationBuilder.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
configuration = configurationBuilder.Build();

// use serilog to console
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

logger.Information("ENVIRONMENT");
logger.Information(env);
logger.Information(connectionString);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddLayersInjector(configuration);

builder.Services.AddCors(options => options.AddPolicy(CORS_NAME, p => p.AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader()));

builder.Services.AddMvc();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}

app.UseExceptionHandler(builder =>
{
    builder.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            context.Response.AddApplicationError(error.Error.Message);
            await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
        }
    });
});

app.UseCors(CORS_NAME);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseIdentityServer();

app.UseCookiePolicy();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();