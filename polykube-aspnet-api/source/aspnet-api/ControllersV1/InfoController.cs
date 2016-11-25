using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Api.ContractsV1;

namespace Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("/info")]
    public class InfoController
    {
        private ILogger logger;

        public InfoController(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(nameof(InfoController));
        }

        [HttpGet()]
        public InfoContract Index()
        {
            return new InfoContract
            {
                Hostname = Environment.MachineName,
            };
        }
    }
}
