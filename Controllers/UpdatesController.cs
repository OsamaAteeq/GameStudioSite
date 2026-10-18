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
            var post = _blogPostRepository.GetBySlug(slug);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
    }
}