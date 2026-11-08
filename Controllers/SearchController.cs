using GameStudioSite.Data;
using GameStudioSite.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStudioSite.Controllers
{
    public enum ResultKind { Post, Game, Page }

    public class SearchResult
    {
        public ResultKind Kind { get; init; }
        public string Title { get; init; } = "";
        public string Url { get; init; } = "";
        public bool External { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageAlt { get; init; }
        public string Label { get; init; } = "";
        public string? Host { get; init; }
        public DateTime? Date { get; init; }
        public List<string> Tags { get; init; } = new();
        public string Description { get; init; } = "";
        public int Score { get; init; }
    }

    public class SearchViewModel
    {
        public string Query { get; set; } = "";
        public bool Searched { get; set; }
        public List<SearchResult> Results { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class SearchController : Controller
    {
        private const int PageSize = 6;   // match your page size
        private readonly BlogPostRepository _posts;
        private readonly GameRepository _games;
        private readonly IConfiguration _config;

        public SearchController(BlogPostRepository posts, GameRepository games, IConfiguration config)
        {
            _posts = posts;
            _games = games;
            _config = config;
        }

        [HttpGet("/Search")]
        public IActionResult Index(string? q, int page = 1)
        {
            Response.Headers["X-Robots-Tag"] = "noindex";

            q = (q ?? "").Trim();
            if (q.Length > 100) q = q[..100];

            var terms = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Where(t => t.Length >= 2)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Take(5)
                         .ToArray();

            var vm = new SearchViewModel { Query = q, Searched = terms.Length > 0 };
            if (vm.Searched)
            {
                var excluded = _config.GetSection("Search:ExcludedCategories").Get<string[]>()
                               ?? Array.Empty<string>();

                var all = SiteSearch.Run(_posts.GetAllExcluding(excluded), _games.GetAll(), terms);

                vm.Total = all.Count;
                vm.TotalPages = Math.Max(1, (int)Math.Ceiling(all.Count / (double)PageSize));
                vm.Page = Math.Clamp(page, 1, vm.TotalPages);
                vm.Results = all.Skip((vm.Page - 1) * PageSize).Take(PageSize).ToList();
            }
            return View(vm);
        }
    }
}