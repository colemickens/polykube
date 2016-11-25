using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.EntityFrameworkCore;
using Api.DataAccess;

namespace Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var listenAddressEnv = Environment.GetEnvironmentVariable("LISTEN_HOST");
            var listenPortEnv = Environment.GetEnvironmentVariable("LISTEN_PORT");

            string listenAddress = !String.IsNullOrEmpty(listenAddressEnv) ? listenAddressEnv : "0.0.0.0";
            int listenPort = !String.IsNullOrEmpty(listenPortEnv) ? Int32.Parse(listenPortEnv) : 9000;

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(new[]{ $"http://{listenAddress}:{listenPort}" })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
