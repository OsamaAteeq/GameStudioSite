using GameStudioSite.Models;
using GameStudioSite.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStudioSite.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            IEmailService emailService,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [Route("/Contact")]
        [HttpGet]
        public IActionResult Index(string? subject)
        {
            // Arrived via a service button: stash the value and redirect to the clean URL
            if (!string.IsNullOrEmpty(subject))
            {
                TempData["PrefillSubject"] = subject;
                return RedirectToAction("Index");
            }

            ViewBag.Submitted = TempData["ContactFormSubmitted"] != null;

            // Reading TempData consumes it, so a reload sees nothing and the dropdown starts empty
            var validSubjects = ServiceSeedData.All.Select(s => $"Services: {s.Title}");
            validSubjects = validSubjects.Concat(ContactFormModel.SubjectOptions);

            var prefillValue = TempData["PrefillSubject"] as string;
            var prefill = prefillValue != null && validSubjects.Contains(prefillValue) ? prefillValue : "";

            return View(new ContactFormModel { Subject = prefill });
        }

        [Route("/Contact")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactFormModel form)
        {
            if (!ModelState.IsValid)
            {
                return View(form);
            }

            var turnstileToken = Request.Form["cf-turnstile-response"];
            var isHuman = await VerifyTurnstileAsync(turnstileToken);

            if (!isHuman)
            {
                ModelState.AddModelError("", "Verification failed. Please try again.");
                return View(form);
            }

            try
            {
                await _emailService.SendContactFormEmailAsync(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact form email.");
                ModelState.AddModelError("", "Something went wrong sending your message. Please try again later.");
                return View(form);
            }

            TempData["ContactFormSubmitted"] = true;
            return Redirect("/Contact#contact-form"); // redirects to /Contact, not a separate Success page
        }

        private async Task<bool> VerifyTurnstileAsync(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var client = _httpClientFactory.CreateClient();
            var secretKey = _config["Turnstile:SecretKey"];

            var response = await client.PostAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secretKey ?? "",
                    ["response"] = token
                }));

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadFromJsonAsync<TurnstileVerifyResponse>();
            return json?.Success ?? false;
        }
    }

    public class TurnstileVerifyResponse
    {
        public bool Success { get; set; }
    }
}