using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Api.Filters;

namespace Api.Controllers
{
    [InstrumentingFilter]
    public class BaseController
    {
        public ILogger logger;

        public BaseController(ILogger logger)
        {
            this.logger = logger;
        }
    }
}
