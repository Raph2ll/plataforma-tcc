using client.Db;
using client.Db.Repositories.Interfaces;
using client.Db.Repositories;
using client.Services.Interfaces;
using client.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Serilog;


namespace client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddSingleton<DbContext>(_ =>
            {
                return new DbContext(connectionString, "client", Log.Logger);
            });

            builder.Services.AddSingleton<IClientRepository, ClientRepository>();
            builder.Services.AddSingleton<IClientService, ClientService>();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Client",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Raph2ll",
                        Email = "raph2ll@gmail.com",
                        Url = new Uri("https://github.com/Raph2ll")
                    }
                });

                c.AddServer(new OpenApiServer
                {
                    Url = "/client",
                    Description = "Base path for client, because of nginx reverse proxy"
                });
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddControllers();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate:
                "{Timestamp:yyyy-MM-ddTHH:mm:ssZ} {Level:u}\t{Message:lj} {NewLine}{Exception}")
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(dispose: true);
            });

            builder.Host.UseSerilog();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "client");
                });
            }

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.MapControllers();

            app.Run($"http://0.0.0.0:8080");
        }
    }
}
