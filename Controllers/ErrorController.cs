using GameStudioSite.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameStudioSite.Controllers
{
    public class ErrorController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public ErrorController(IWebHostEnvironment env) => _env = env;

        [Route("Error/{code:int}")]
        public IActionResult Status(int code)
        {
            var reExecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();

            // Opened by typing the address: treat it as a page that doesn't exist.
            // Development is exempt so you can still preview and style these pages.
            if (reExecute is null && exception is null && !_env.IsDevelopment())
                return NotFound();

            Response.Headers["X-Robots-Tag"] = "noindex";

            // Missing files and scanner probes (.png, .env, .php...) get an empty
            // response with the right status instead of a full HTML page
            var original = reExecute?.OriginalPath ?? exception?.Path ?? "";

            if (Path.HasExtension(original)) return StatusCode(code);

            ViewData["ScrollTo"] = "main";
            if (code == 404) return View("PageNotFound");
            return View("Generic", ErrorPageModel.For(code));
        }
    }
}