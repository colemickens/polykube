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

        public async void ConfigureServices(IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            this.Configuration = configBuilder.Build();

            services.AddOptions();

            var redis = await configureRedis();
            services.AddSingleton<IDatabase>((IServiceCollection) => redis);
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
                var database = this.Configuration["postgres_database"];
                var username = this.Configuration["postgres_username"];
                var password = this.Configuration["postgres_password"];

                var connectionString = $"server={host};port={port};user id={username};password={password};database={database}";

                var npgsql = options.UseNpgsql(connectionString);

                Console.WriteLine("POSTGRES: Connected!");
                return npgsql;
            } catch(Exception e) {
                Console.Error.WriteLine("POSTGRES: Failed to connect.");
                Console.Error.WriteLine("POSTGRES: Exception: {0}", e);
                return null;
            }
        }

        private async Task<IDatabase> configureRedis()
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
                    var possibleHosts = await Dns.GetHostAddressesAsync(host);
                    host = possibleHosts.First(h => h.AddressFamily == AddressFamily.InterNetwork).ToString();
                }
                // end workaround

                var connectionString = $"{host}:{port},password={password}";

                var redis = ConnectionMultiplexer.Connect(connectionString, connectionOutput);
                Console.Error.WriteLine(connectionOutput.ToString());
                var db = redis.GetDatabase();


                Console.WriteLine("REDIS: Connected!");

                return db;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("REDIS: Connection Failed!");
                Console.Error.WriteLine(e);
                throw;
            }
        }
    }
}
