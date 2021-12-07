using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace StellarShowcase.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected async Task<ActionResult> HandleRequest(Func<Task> handleFn)
        {
            try
            {
                await handleFn();
                return Ok();
            }
            catch (Exception ex)
            {
                return HandlerEx(ex);
            }
        }

        protected async Task<ActionResult<T>> HandleRequest<T>(Func<Task<T>> handleFn)
        {
            try
            {
                var result = await handleFn();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandlerEx(ex);
            }
        }

        private ActionResult HandlerEx(Exception ex)
        {
            return ex switch
            {
                ArgumentException argumentEx => BadRequest(argumentEx.Message),
                UnauthorizedAccessException unauthorizedAccessEx => Unauthorized(unauthorizedAccessEx.Message),
                _ => new StatusCodeResult(500),
            };
        }
    }
}
