using GameStudioSite.Data;
using GameStudioSite.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameStudioSite.Controllers
{
    public class UpdatesController : Controller
    {

        private readonly BlogPostRepository _blogPostRepository;
        private const int PageSize = 6;

        public UpdatesController(BlogPostRepository blogPostRepository)
        {
            _blogPostRepository = blogPostRepository;
        }

        public IActionResult Index(int page = 1, string? category = null)
        {
            if (page < 1) page = 1;

            var (posts, totalCount) = _blogPostRepository.GetPage(page, PageSize, category);

            var viewModel = new BlogIndexViewModel
            {
                Posts = posts,
                Categories = _blogPostRepository.GetAllCategoriesPublic(),
                SelectedCategory = category,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
            };

            return View(viewModel);
        }

        [Route("{category}/{slug}")]
        [HttpGet]
        public IActionResult Details(string category, string slug)
        {
            var post = _blogPostRepository.GetByCategoryAndSlug(category, slug);
            if (post == null)
            {
                return NotFound();
            }
            var canonicalCategory = CategoryRepository.Slugify(post.Category);
            if (!string.Equals(category, canonicalCategory, StringComparison.Ordinal))
            {
                return RedirectPermanent($"/{canonicalCategory}/{post.Slug}");
            }
            return View(post);
        }

        [HttpGet("/{category}", Order = 1000)]
        public IActionResult CategoryRedirect(string category)
        {
            var wanted = CategoryRepository.Slugify(category);

            // Only public categories that have posts
            var match = _blogPostRepository.GetAllCategoriesPublic()
                .FirstOrDefault(c => string.Equals(
                    CategoryRepository.Slugify(c), wanted, StringComparison.OrdinalIgnoreCase));

            if (match is null) return NotFound();

            return Redirect($"/Updates?category={Uri.EscapeDataString(match)}#updates-content");
        }
    }
}