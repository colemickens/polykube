using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Api.Contracts.V1;

namespace Api.Controllers
{
    [Route("/info")]
    public class InfoController : BaseController
    {
        public InfoController(ILoggerFactory loggerFactory)
            : base(loggerFactory.CreateLogger(nameof(InfoController)))
        {
        }

        [HttpGet()]
        public string Index()
        {
            return Environment.MachineName;
        }
    }
}
