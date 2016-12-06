using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
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

namespace Api
{
    public class Startup
    {
        private IConfiguration Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            this.Configuration = configBuilder.Build();

            services.AddOptions();

            services.AddSingleton<IDatabase>((IServiceCollection) => configureRedis());
            services.AddDbContext<DataContext>(options => configureDatabase(options));
            //services.AddMvcCore().AddJsonFormatters();
            services.AddMvc();
            //services.AddApiVersioning(o => o.AssumeDefaultVersionWhenUnspecified = false);
            services.AddApiVersioning();
            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DataContext dataContext/*, IDatabase redisDb*/)
        {
            dataContext.Database.EnsureCreatedAsync().Wait();

            if (env.IsDevelopment()) {
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
            try {
                var host = this.Configuration["postgres_host"];
                var port = this.Configuration["postgres_port"];
                var dbname = this.Configuration["database_name"];
                var username = this.Configuration["postgres_username"];
                var password = this.Configuration["postgres_password"];

                var connectionString = $"server={host};port={port};user id={username};password={password};database={dbname}";
                Console.WriteLine($"postgres connstring: {connectionString}");

                var npgsql = options.UseNpgsql(connectionString);

                Console.WriteLine("POSTGRES: Connected!");
                return npgsql;
            } catch(Exception e) {
                Console.WriteLine("POSTGRES: Failed to connect.");
                Console.WriteLine("POSTGRES: Exception: {0}", e);
                return null;
            }
        }

        private IDatabase configureRedis()
        {
            var connectionOutput = new StringWriter();
            try {
                string host = this.Configuration["redis_host"];
                string port = this.Configuration["redis_port"];
                string password = this.Configuration["redis_password"];

                // start workaround: BUG_LINK
                // manually resolve dns
                if (IPAddress.TryParse(host, out var _)) {
                    Console.WriteLine("REDIS: parsed as ip, using as-is");
                } else {
                    Console.WriteLine($"REDIS: resolving '{host}'");
                    var possibleHosts = Dns.GetHostAddressesAsync(host);
                    host = possibleHosts.Result.First(h => h.AddressFamily == AddressFamily.InterNetwork).ToString();
                }
                // end workaround

                var connectionString = $"{host}:{port},password={password}";
                Console.WriteLine($"REDIS: connstring={connectionString}");

				var stamp = "Abc123xyz";

                Console.WriteLine($"REDIS: Connecting... {stamp}");
                var redis = ConnectionMultiplexer.Connect(connectionString, connectionOutput);
                Console.WriteLine($"REDIS: Connecting...");
                Console.WriteLine($"REDIS: Output:");
                Console.WriteLine(connectionOutput.ToString());
                var db = redis.GetDatabase();

                Console.WriteLine("REDIS: Connected!");

                return db;
            }
            catch (Exception e)
            {
                Console.WriteLine($"REDIS: Exception!");
                Console.WriteLine($"REDIS: Output:");
                Console.WriteLine(connectionOutput.ToString());
                Console.WriteLine($"REDIS: Connection Failed!");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
