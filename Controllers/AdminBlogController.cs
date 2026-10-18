using GameStudioSite.Data;
using GameStudioSite.Models;
using GameStudioSite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStudioSite.Controllers
{
    [Authorize]
    [Route("/Admin/Updates")]
    public class AdminBlogController : Controller
    {
        private readonly BlogPostRepository _blogPostRepository;
        private readonly ImageUploadService _imageUploadService;
        private readonly CategoryRepository _categoryRepository;

        public AdminBlogController(BlogPostRepository blogPostRepository, ImageUploadService imageUploadService, CategoryRepository categoryRepository)
        {
            _blogPostRepository = blogPostRepository;
            _imageUploadService = imageUploadService;
            _categoryRepository = categoryRepository;
        }

        [Route("Categories/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(string name, bool isPublic)
        {
            name = name?.Trim() ?? "";

            if (string.IsNullOrEmpty(name))
            {
                TempData["AdminMessage"] = "Category name can't be empty.";
            }
            else if (_categoryRepository.NameExists(name))
            {
                var normalized = CategoryRepository.Normalize(name);
                TempData["AdminMessage"] = $"\"{normalized}\" already exists.";
            }
            else
            {
                _categoryRepository.Create(name, isPublic);
                var normalized = CategoryRepository.Normalize(name);
                TempData["AdminMessage"] = $"Category \"{normalized}\" added.";
            }

            return RedirectToAction("Index");
        }

        [Route("Categories/Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(string id)
        {
            _categoryRepository.Delete(id);
            TempData["AdminMessage"] = "Category deleted.";
            return RedirectToAction("Index");
        }

        [Route("")]
        public IActionResult Index(string? sort)
        {
            var oldestFirst = sort == "oldest";

            var posts = _blogPostRepository.GetAllForAdmin(); // newest first by default
            if (oldestFirst)
            {
                posts = posts.OrderBy(p => p.PublishedDate).ToList();
            }

            var categories = _categoryRepository.GetAll();

            var viewModel = new AdminBlogIndexViewModel
            {
                Posts = posts,
                Categories = categories,
                PrivateCategoryNames = categories.Where(c => !c.IsPublic).Select(c => c.Name).ToHashSet(),
                OldestFirst = oldestFirst
            };
            return View(viewModel);
        }

        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminBlogPostPageViewModel
            {
                Form = new AdminBlogPostFormModel(),
                Categories = _categoryRepository.GetAll()
            });
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminBlogPostPageViewModel model)
        {
            var form = model.Form;
            if (!ModelState.IsValid)
            {
                model.Categories = _categoryRepository.GetAll();
                return View(model);
            }

            string? imageUrl;
            try
            {
                imageUrl = await _imageUploadService.SaveImageAsync(form.ImageFile, "blog");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("Form.ImageFile", ex.Message);
                model.Categories = _categoryRepository.GetAll();
                return View(model);
            }

            if (imageUrl == null)
            {
                ModelState.AddModelError("Form.ImageFile", "Please choose a featured image.");
                model.Categories = _categoryRepository.GetAll();
                return View(model);
            }

            var post = new BlogPost
            {
                Title = form.Title,
                Slug = form.Slug,
                Excerpt = form.Excerpt,
                ContentHtml = form.ContentHtml,
                ImageAltText = form.ImageAltText,
                Category = form.Category,
                Tags = SplitTags(form.TagsInput),
                PublishedDate = DateTime.SpecifyKind(form.PublishedDate, DateTimeKind.Utc),
                FeaturedImageUrl = imageUrl,
                ShowImage = form.ShowImage
            };

            _blogPostRepository.Create(post);
            TempData["AdminMessage"] = $"\"{post.Title}\" was created.";
            return RedirectToAction("Index");
        }

        [Route("Edit/{id}")]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var post = _blogPostRepository.GetById(id);
            if (post == null)
            {
                return NotFound();
            }

            var form = new AdminBlogPostFormModel
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                ContentHtml = post.ContentHtml,
                ImageAltText = post.ImageAltText,
                Category = post.Category,
                TagsInput = string.Join(", ", post.Tags),
                PublishedDate = post.PublishedDate,
                ExistingImageUrl = post.FeaturedImageUrl,
                ShowImage = post.ShowImage
            };

            return View(new AdminBlogPostPageViewModel
            {
                Form = form,
                Categories = _categoryRepository.GetAll()
            });
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminBlogPostPageViewModel model)
        {
            var form = model.Form;
            if (!ModelState.IsValid)
            {
                model.Categories = _categoryRepository.GetAll();
                return View(model);
            }

            var imageUrl = form.ExistingImageUrl;

            if (form.ImageFile != null)
            {
                string? newImageUrl;
                try
                {
                    newImageUrl = await _imageUploadService.SaveImageAsync(form.ImageFile, "blog");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Form.ImageFile", ex.Message);
                    model.Categories = _categoryRepository.GetAll();
                    return View(model);
                }

                if (newImageUrl != null)
                {
                    _imageUploadService.DeleteImage(form.ExistingImageUrl);
                    imageUrl = newImageUrl;
                }
            }

            var post = new BlogPost
            {
                Id = id,
                Title = form.Title,
                Slug = form.Slug,
                Excerpt = form.Excerpt,
                ContentHtml = form.ContentHtml,
                ImageAltText = form.ImageAltText,
                Category = form.Category,
                Tags = SplitTags(form.TagsInput),
                PublishedDate = DateTime.SpecifyKind(form.PublishedDate, DateTimeKind.Utc),
                FeaturedImageUrl = imageUrl!,
                ShowImage = form.ShowImage
            };

            _blogPostRepository.Update(id, post);
            TempData["AdminMessage"] = $"\"{post.Title}\" was updated.";
            return RedirectToAction("Index");
        }

        [Route("Delete/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id, string? sort)
        {
            var post = _blogPostRepository.GetById(id);
            if (post != null)
            {
                _imageUploadService.DeleteImage(post.FeaturedImageUrl);
                _blogPostRepository.Delete(id);
                TempData["AdminMessage"] = $"\"{post.Title}\" was deleted.";
            }

            return RedirectToAction("Index", new { sort });
        }

        private static List<string> SplitTags(string input)
        {
            return input
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }
    }
}