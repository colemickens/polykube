using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Api.Contracts.V1;

namespace Api.Controllers
{
    [Route("/sha256")]
    public class Sha256Controller : BaseController
    {
        public Sha256Controller(ILoggerFactory loggerFactory)
            : base(loggerFactory.CreateLogger(nameof(Sha256Controller)))
        {
        }

        [HttpGet()]
        public string Index(string input)
        {
            if (input == null) { return "no input"; }

            var toHash = input;
            var toHashBytes = System.Text.Encoding.UTF8.GetBytes(toHash);

            var sha256 = SHA256.Create();
            var hashSum = sha256.ComputeHash(toHashBytes);

            return BitConverter.ToString(hashSum).Replace("-", string.Empty);
        }
    }
}
