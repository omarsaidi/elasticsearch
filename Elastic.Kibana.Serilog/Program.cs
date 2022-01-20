ing Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



private static void ConfigureLogging()
{
	var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
	var configuration = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		.AddJsonFile(
			$"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
			optional: true)
		.Build();

	Log.Logger = new LoggerConfiguration()
		.Enrich.FromLogContext()
		.Enrich.WithMachineName()
		.WriteTo.Debug()
		.WriteTo.Console()
		.WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
		.Enrich.WithProperty("Environment", environment)
		.ReadFrom.Configuration(configuration)
		.CreateLogger();
}

private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
	return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
	{
		AutoRegisterTemplate = true,
		IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
	};
}
