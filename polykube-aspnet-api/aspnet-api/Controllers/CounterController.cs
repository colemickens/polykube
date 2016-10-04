using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Api.Contracts.V1;
using Api.DataAccess;
using Api.Models;
using Api.Services.Color;
using Api.Services.Counter;

namespace Api.Controllers
{
    [Route("/counter")]
    public class CounterController : BaseController
    {
        private DataContext dataContext;
        private IDatabase redisDb;
        private ICounterService counterService;
        private IColorService colorService;

        public CounterController(
                ILoggerFactory loggerFactory,
                DataContext dataContext,
                IDatabase redisDb,
                ICounterService counterService,
                IColorService colorService)
            : base(loggerFactory.CreateLogger(nameof(CounterController)))
        {
            this.dataContext = dataContext;
            this.redisDb = redisDb;
            this.counterService = counterService;
            this.colorService = colorService;
        }

        [HttpGet()]
        public CounterContract Index()
        {
            string hostname = Environment.MachineName;
            long instanceCount = counterService.Increment();
            long globalCount = redisDb.StringIncrement("COUNTER");

            Color c = colorService.GetColorShade();
            string shade = $"rgb({c.Red}, {c.Green}, {c.Blue})";

            var demoResponse = new CounterContract
            {
                Hostname = hostname,
                Shade = shade,
                InstanceCount = instanceCount,
                GlobalCount = globalCount,
            };

            return demoResponse;
        }
    }
}
