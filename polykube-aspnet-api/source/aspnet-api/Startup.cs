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
using Microsoft.Data.Sqlite;
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
			switch (this.Configuration["db_driver"]) {
				case "sqlite":
            		services.AddDbContext<DataContext>(options => configureSqlite(options));
					break;
				case "postgres":
					break;
            		services.AddDbContext<DataContext>(options => configureSqlite(options));
				case "mssql":
            		services.AddDbContext<DataContext>(options => configureSqlite(options));
					break;
				default:
					throw new NotSupportedOperation("must specify a valid db_driver")
			}

            //services.AddDbContext<DataContext>(options => configurePostgres(options));
            //services.AddDbContext<DataContext>(options => configureMssql(options));
            services.AddMvc();
            services.AddApiVersioning();
            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DataContext dataContext, IDatabase redisDb)
        {
        	Console.WriteLine($"DB: EnsureCreatedAsync()-ing");
            dataContext.Database.EnsureCreated();
        	Console.WriteLine($"DB: EnsureCreatedAsync()-d");

            if (env.IsDevelopment()) {
                loggerFactory.AddConsole(LogLevel.Debug);
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
                app.UseDatabaseErrorPage();
            } else {
                loggerFactory.AddConsole(LogLevel.Warning);
            }

            app.UseCors(policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod());
            app.UseMvc();
        }

        private DbContextOptionsBuilder configurePostgres(DbContextOptionsBuilder options)
		{
            try {
                var host = this.Configuration["mssql_host"];
                var port = this.Configuration["mssql_port"];
                var dbname = this.Configuration["database_name"];
                var username = this.Configuration["mssql_username"];
                var password = this.Configuration["mssql_password"];

                //var connectionString = $"server={host};port={port};user id={username};password={password};database={dbname}";
                var connectionString = $"Server={host},{port};Database={dbname};User Id={username};Password={password};";
                Console.WriteLine($"MSSQL connstring: {connectionString}");

				//var npgsql = options.UseNpgsql(connectionString);
				//var mssql = options.UseSqlServer(connectionString);
	            var connection = new SqliteConnection("DataSource=:memory:");
				var sqlite = options.UseSqlite(connection);

                Console.WriteLine("MSSQL: Connected!");
                //return npgsql;
                //return mssql;
				return sqlite;
            } catch(Exception e) {
                Console.WriteLine("MSSQL: Failed to connect.");
                Console.WriteLine("MSSQL: Exception: {0}", e);
                return null;
            }
		}
        private DbContextOptionsBuilder configureSqlite(DbContextOptionsBuilder options)
		{
            try {
                var host = this.Configuration["mssql_host"];
                var port = this.Configuration["mssql_port"];
                var dbname = this.Configuration["database_name"];
                var username = this.Configuration["mssql_username"];
                var password = this.Configuration["mssql_password"];

                //var connectionString = $"server={host};port={port};user id={username};password={password};database={dbname}";
                var connectionString = $"Server={host},{port};Database={dbname};User Id={username};Password={password};";
                Console.WriteLine($"MSSQL connstring: {connectionString}");

				//var npgsql = options.UseNpgsql(connectionString);
				//var mssql = options.UseSqlServer(connectionString);
	            var connection = new SqliteConnection("DataSource=:memory:");
				var sqlite = options.UseSqlite(connection);

                Console.WriteLine("MSSQL: Connected!");
                //return npgsql;
                //return mssql;
				return sqlite;
            } catch(Exception e) {
                Console.WriteLine("MSSQL: Failed to connect.");
                Console.WriteLine("MSSQL: Exception: {0}", e);
                return null;
            }
		}

        private DbContextOptionsBuilder configureMssql(DbContextOptionsBuilder options)
        {
            try {
                var host = this.Configuration["mssql_host"];
                var port = this.Configuration["mssql_port"];
                var dbname = this.Configuration["database_name"];
                var username = this.Configuration["mssql_username"];
                var password = this.Configuration["mssql_password"];

                //var connectionString = $"server={host};port={port};user id={username};password={password};database={dbname}";
                var connectionString = $"Server={host},{port};Database={dbname};User Id={username};Password={password};";
                Console.WriteLine($"MSSQL connstring: {connectionString}");

				//var npgsql = options.UseNpgsql(connectionString);
				//var mssql = options.UseSqlServer(connectionString);
	            var connection = new SqliteConnection("DataSource=:memory:");
				var sqlite = options.UseSqlite(connection);

                Console.WriteLine("MSSQL: Connected!");
                //return npgsql;
                //return mssql;
				return sqlite;
            } catch(Exception e) {
                Console.WriteLine("MSSQL: Failed to connect.");
                Console.WriteLine("MSSQL: Exception: {0}", e);
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
