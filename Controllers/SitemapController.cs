using GameStudioSite.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace GameStudioSite.Controllers
{
    public class SitemapController : Controller
    {
        private readonly BlogPostRepository _posts;
        private readonly IConfiguration _config;

        public SitemapController(BlogPostRepository posts, IConfiguration config)
        {
            _posts = posts;
            _config = config;
        }

        [HttpGet("/sitemap.xml")]
        [ResponseCache(Duration = 3600)]
        public IActionResult Index()
        {
            var baseUrl = _config["Site:BaseUrl"]!.TrimEnd('/');
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var urls = new List<XElement>();

            void Add(string path, DateTime? lastMod = null)
            {
                var url = new XElement(ns + "url", new XElement(ns + "loc", baseUrl + path));
                if (lastMod is not null)
                    url.Add(new XElement(ns + "lastmod", lastMod.Value.ToString("yyyy-MM-dd")));
                urls.Add(url);
            }

            foreach (var path in new[] { "/", "/Games", "/Services", "/About", "/Updates", "/Contact" })
                Add(path);

            var posts = _posts.GetAllForAdmin()
                .Where(p => !p.Category.Equals("Test", StringComparison.OrdinalIgnoreCase));

            foreach (var post in posts)
                Add($"/{CategoryRepository.Slugify(post.Category)}/{post.Slug}", post.PublishedDate);

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "urlset", urls));

            return Content(doc.ToString(), "application/xml", Encoding.UTF8);
        }
    }
}