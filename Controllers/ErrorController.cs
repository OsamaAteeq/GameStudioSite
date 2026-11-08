using GameStudioSite.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameStudioSite.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{code:int}")]
        public IActionResult Status(int code)
        {
            Response.Headers["X-Robots-Tag"] = "noindex";

            // Missing files and scanner probes (.png, .env, .php...) get an empty
            // response with the right status instead of a full HTML page
            var original = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath ?? "";
            if (Path.HasExtension(original)) return StatusCode(code);
            ViewData["ScrollTo"] = "main";
            if (code == 404) return View("PageNotFound");
            return View("Generic", ErrorPageModel.For(code));
        }
    }
}