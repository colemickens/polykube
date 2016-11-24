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
            var listenAddressEnv = Environment.GetEnvironmentVariable("POLYKUBE_LISTEN_ADDRESS");
            var listenPortEnv = Environment.GetEnvironmentVariable("POLYKUBE_LISTEN_PORT");

            string listenAddress = !String.IsNullOrEmpty(listenAddressEnv) ? listenAddressEnv : "0.0.0.0";
            int listenPort = !String.IsNullOrEmpty(listenPortEnv) ? Int32.Parse(listenPortEnv) : 8000;

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(new[]{ $"http://{listenAddress}:{listenPort}" })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
