using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters
{
    public class InstrumentingFilter : ActionFilterAttribute
    {
        private Stopwatch requestTimer;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            requestTimer = Stopwatch.StartNew();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var time = requestTimer.ElapsedMilliseconds;
            context.HttpContext.Response.Headers.Add(
                "ActionElapsedTime",
                new string[] { time.ToString(CultureInfo.InvariantCulture) + " ms" });
        }
    }
}
