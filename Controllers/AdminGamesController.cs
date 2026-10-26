using GameStudioSite.Data;
using GameStudioSite.Models;
using GameStudioSite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace GameStudioSite.Controllers
{
    [Authorize]
    [Route("/Admin/Games")]
    public class AdminGamesController : Controller
    {
        private readonly GameRepository _gameRepository;
        private readonly ImageUploadService _imageUploadService;
        private readonly BlogPostRepository _blogPostRepository;
        private readonly IHttpClientFactory _httpClientFactory;


        private async Task<string?> ValidateGameUrlAsync(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Please enter a game URL.";

            value = value.Trim();

            // Internal URL: /category/slug
            if (value.StartsWith("/") && !value.StartsWith("//"))
            {
                if (!Uri.TryCreate(value, UriKind.Relative, out var relativeUri))
                    return "Enter a valid internal URL.";

                var path = relativeUri.OriginalString
                    .Split('?', '#')[0]
                    .Trim('/');

                var segments = path.Split('/');

                if (segments.Length != 2 ||
                    segments.Any(string.IsNullOrWhiteSpace))
                {
                    return "Internal links must use /category/slug.";
                }

                var category = Uri.UnescapeDataString(segments[0]);
                var slug = Uri.UnescapeDataString(segments[1]);

                if (category.Contains('/') || slug.Contains('/'))
                    return "The internal URL contains an invalid path.";

                var post = _blogPostRepository
                    .GetByCategoryAndSlug(category, slug);

                return post == null
                    ? "No blog post exists at this internal URL."
                    : null;
            }

            // External URL: HTTP or HTTPS only
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps) ||
                string.IsNullOrWhiteSpace(uri.Host) ||
                !string.IsNullOrEmpty(uri.UserInfo))
            {
                return "Enter an internal path or a valid HTTP/HTTPS URL.";
            }

            // Reject localhost and private/local IP destinations.
            // DNS and redirect destinations also need checking before
            // making requests to arbitrary external URLs.
            return await CheckExternalUrlAsync(uri);
        }

        private async Task<string?> CheckExternalUrlAsync(Uri initialUri)
        {
            var client = _httpClientFactory.CreateClient("GameUrlValidation");
            client.Timeout = TimeSpan.FromSeconds(8);

            var currentUri = initialUri;
            var method = HttpMethod.Head;
            var redirects = 0;

            try
            {
                while (true)
                {
                    using var request = new HttpRequestMessage(method, currentUri);
                    using var response = await client.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead);

                    // Some websites do not support HEAD; retry using GET.
                    if (method == HttpMethod.Head &&
                        (response.StatusCode ==
                            System.Net.HttpStatusCode.MethodNotAllowed ||
                         response.StatusCode ==
                            System.Net.HttpStatusCode.NotImplemented))
                    {
                        method = HttpMethod.Get;
                        continue;
                    }

                    if ((int)response.StatusCode >= 300 &&
                        (int)response.StatusCode < 400)
                    {
                        if (redirects >= 5)
                            return "The external URL redirects too many times.";

                        var location = response.Headers.Location;

                        if (location == null)
                            return "The external URL returned an invalid redirect.";

                        var nextUri = location.IsAbsoluteUri
                            ? location
                            : new Uri(currentUri, location);

                        if (!IsAllowedExternalUri(nextUri))
                        {
                            return "The external URL redirects to an invalid destination.";
                        }

                        currentUri = nextUri;
                        redirects++;
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                        return "The external URL could not be reached successfully.";

                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return "The URL could not be reached, or its destination is not allowed.";
            }
            catch (SocketException)
            {
                return "Could not connect to the external URL.";
            }
            catch (TaskCanceledException)
            {
                return "The external URL took too long to respond.";
            }
            catch (UriFormatException)
            {
                return "The external URL returned an invalid redirect.";
            }
            catch (Exception ex)
            {
                return "An unexpected error occurred while validating the external URL.";
            }
        }

        private static bool IsAllowedExternalUri(Uri uri)
        {
            return uri.IsAbsoluteUri
                && (uri.Scheme == Uri.UriSchemeHttp ||
                    uri.Scheme == Uri.UriSchemeHttps)
                && !string.IsNullOrWhiteSpace(uri.Host)
                && string.IsNullOrEmpty(uri.UserInfo);
        }


        private static bool IsValidGameUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            url = url.Trim();

            // Internal URL: must be a root-relative path
            if (url.StartsWith("/") && !url.StartsWith("//"))
            {
                return Uri.TryCreate(url, UriKind.Relative, out _);
            }

            // External URL: allow only HTTP or HTTPS
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp
                    || uri.Scheme == Uri.UriSchemeHttps)
                && !string.IsNullOrWhiteSpace(uri.Host);
        }

        public AdminGamesController(GameRepository gameRepository, ImageUploadService imageUploadService, IHttpClientFactory httpClientFactory, BlogPostRepository blogPostRepository)
        {
            _gameRepository = gameRepository;
            _imageUploadService = imageUploadService;
            _httpClientFactory = httpClientFactory;
            _blogPostRepository = blogPostRepository;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View(_gameRepository.GetAll());
        }

        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminGameFormModel());
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminGameFormModel form)
        {
            if (!IsValidGameUrl(form.GameUrl))
            {
                ModelState.AddModelError(
                    nameof(form.GameUrl),
                    "Enter an internal path (e.g. /games/my-game) or a valid Http/Https URL."
                );
            }
            else
            {
                var urlError = await ValidateGameUrlAsync(form.GameUrl);

                if (urlError != null)
                {
                    ModelState.AddModelError(nameof(form.GameUrl), urlError);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(form);
            }

            string? imageUrl;
            try
            {
                imageUrl = await _imageUploadService.SaveImageAsync(form.ImageFile, "games");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(form.ImageFile), ex.Message);
                return View(form);
            }

            if (imageUrl == null)
            {
                ModelState.AddModelError(nameof(form.ImageFile), "Please choose an image.");
                return View(form);
            }

            var game = new Game
            {
                Title = form.Title,
                Slug = form.Slug,
                Description = form.Description,
                GameUrl = form.GameUrl,
                ImageUrl = imageUrl
            };

            _gameRepository.Create(game);
            TempData["AdminMessage"] = $"\"{game.Title}\" was created.";
            return RedirectToAction("Index");
        }

        [Route("Edit/{id}")]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var game = _gameRepository.GetById(id);
            if (game == null)
            {
                return NotFound();
            }

            var form = new AdminGameFormModel
            {
                Id = game.Id,
                Title = game.Title,
                Slug = game.Slug,
                Description = game.Description,
                GameUrl = game.GameUrl,
                ExistingImageUrl = game.ImageUrl
            };

            return View(form);
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminGameFormModel form)
        {
            if (!IsValidGameUrl(form.GameUrl))
            {
                ModelState.AddModelError(
                    nameof(form.GameUrl),
                    "Enter an internal path (e.g. /games/my-game) or a valid Http/Https URL."
                );
            }
            else
            {
                var urlError = await ValidateGameUrlAsync(form.GameUrl);

                if (urlError != null)
                {
                    ModelState.AddModelError(nameof(form.GameUrl), urlError);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(form);
            }

            var imageUrl = form.ExistingImageUrl;

            if (form.ImageFile != null)
            {
                string? newImageUrl;
                try
                {
                    newImageUrl = await _imageUploadService.SaveImageAsync(form.ImageFile, "games");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(form.ImageFile), ex.Message);
                    return View(form);
                }

                if (newImageUrl != null)
                {
                    _imageUploadService.DeleteImage(form.ExistingImageUrl); // remove the old file now that the new one's saved
                    imageUrl = newImageUrl;
                }
            }

            var game = new Game
            {
                Id = id,
                Title = form.Title,
                Slug = form.Slug,
                Description = form.Description,
                GameUrl = form.GameUrl,
                ImageUrl = imageUrl!
            };

            _gameRepository.Update(id, game);
            TempData["AdminMessage"] = $"\"{game.Title}\" was updated.";
            return RedirectToAction("Index");
        }

        [Route("Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var game = _gameRepository.GetById(id);
            if (game != null)
            {
                _imageUploadService.DeleteImage(game.ImageUrl);
                _gameRepository.Delete(id);
                TempData["AdminMessage"] = $"\"{game.Title}\" was deleted.";
            }

            return RedirectToAction("Index");
        }
    }
}