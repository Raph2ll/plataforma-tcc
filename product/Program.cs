using product.Db;
using product.Db.Repositories.Interfaces;
using product.Db.Repositories;
using product.Services.Interfaces;
using product.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Serilog;
using product.Services.Refit;
using Refit;


namespace product
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
            string quotationApi = configuration["External:QuotationApi"];


            builder.Services.AddSingleton<DbContext>(_ =>
            {
                return new DbContext(connectionString, "product", Log.Logger);
            });

            builder.Services.AddSingleton<IProductRepository, ProductRepository>();
            builder.Services.AddSingleton<IProductService, ProductService>();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Product",
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
                    Url = "/product",
                    Description = "Base path for product, because of nginx reverse proxy"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddControllers();
            builder.Services.AddRefitClient<IQuotation>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(quotationApi));

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
                    c.SwaggerEndpoint("v1/swagger.json", "product");
                });
            }

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.MapControllers();
            app.Run($"http://0.0.0.0:8080");
        }
    }
}