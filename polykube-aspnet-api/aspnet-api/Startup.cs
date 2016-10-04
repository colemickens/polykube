using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

using Api.Controllers;
using Api.DataAccess;
using Api.Services.Color;
using Api.Services.Counter;

namespace Api
{
    public class Startup
    {
        private IConfiguration Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables("polykube_");
            this.Configuration = configBuilder.Build();

            services.AddOptions();

            services.AddSingleton<IDatabase>((IServiceCollection) => configureRedis());
            services.AddSingleton<ICounterService>((IServiceCollection) => new CounterService());
            services.AddSingleton<IColorService>((IServiceCollection) => new ColorService());
            services.AddDbContext<DataContext>(options => configureDatabase(options));

            services.AddMvcCore().AddJsonFormatters(json =>
            {
                json.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DataContext dataContext, IDatabase redisDb)
        {
            ensureDatabase(dataContext);

            if (env.IsDevelopment()) {
                Console.WriteLine("starting in development mode");
                loggerFactory.AddDebug(LogLevel.Information);
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
                app.UseDatabaseErrorPage();
            } else {
                loggerFactory.AddConsole(LogLevel.Warning);
            }

            app.UseCors(policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod());
            app.UseMvc();
        }

        private DbContextOptionsBuilder configureDatabase(DbContextOptionsBuilder options)
        {
            // can I get a logger here?
            // I'm probably wrong for asking, just like I'm wrong to want
            // to use a strongly typed config object
            // while setting up other things that will be injected

            // TODO: remove this when exceptions aren't swallowed here
            try {
            var driver = this.Configuration["database_driver"];
            Console.WriteLine($"db driver: {driver}");

            switch(driver)
            {
                case "postgres":
                {
                    var server = this.Configuration["postgres_address"];
                    var port = this.Configuration["postgres_port"];
                    var database = this.Configuration["database_name"];
                    var username = this.Configuration["postgres_username"];
                    var passwordLocation = this.Configuration["postgres_password_location"];
                    var password = File.ReadAllLines(passwordLocation)[0];

                    var connectionString = $"server={server};user id={username};password={password};database={database}";

                    var npgsql = options.UseNpgsql(connectionString);
                    return npgsql;
                }
                case "memory":
                default:
                {
                    return options.UseInMemoryDatabase();
                }
            }
            } catch(Exception e) {
                Console.Error.WriteLine("Failed to connect to Postgres.");
                Console.Error.WriteLine("Exception:  {0}", e);
                return null;
            }
        }

        private void ensureDatabase(DataContext dataContext)
        {
            dataContext.Database.EnsureCreatedAsync().Wait();
        }

        private IDatabase configureRedis()
        {
            var connectionOutput = new StringWriter();
            try {
                var server = this.Configuration["redis_address"];
                var port = this.Configuration["redis_port"];
                var passwordLocation = this.Configuration["redis_password_location"];
                var password = File.ReadAllLines(passwordLocation)[0];

                var connectionString = $"{server}:{port},password={password}";

                var redis = ConnectionMultiplexer.Connect(connectionString, connectionOutput);
                var db = redis.GetDatabase();
                return db;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to connect to Redis.");
                Console.Error.WriteLine("Connection: {0}", connectionOutput.ToString());
                Console.Error.WriteLine("Exception:  {0}", e);
                throw;
            }
        }
    }
}
