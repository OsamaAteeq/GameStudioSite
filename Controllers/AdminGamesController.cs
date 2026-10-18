using GameStudioSite.Data;
using GameStudioSite.Models;
using GameStudioSite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStudioSite.Controllers
{
    [Authorize]
    [Route("/Admin/Games")]
    public class AdminGamesController : Controller
    {
        private readonly GameRepository _gameRepository;
        private readonly ImageUploadService _imageUploadService;

        public AdminGamesController(GameRepository gameRepository, ImageUploadService imageUploadService)
        {
            _gameRepository = gameRepository;
            _imageUploadService = imageUploadService;
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
                ItchIoUrl = form.ItchIoUrl,
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
                ItchIoUrl = game.ItchIoUrl,
                ExistingImageUrl = game.ImageUrl
            };

            return View(form);
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminGameFormModel form)
        {
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
                ItchIoUrl = form.ItchIoUrl,
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