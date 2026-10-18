using GameStudioSite.Data;
using GameStudioSite.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GameStudioSite.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly GameRepository _gameRepository;
        private readonly BlogPostRepository _blogPostRepository;

        public HomeController(GameRepository gameRepository, BlogPostRepository blogPostRepository)
        {
            _gameRepository = gameRepository;
            _blogPostRepository = blogPostRepository;
        }

        [Route("/")]
        public IActionResult Index()
        {
            var viewModel = new HomeIndexViewModel
            {
                Services = ServiceSeedData.All,
                LatestGames = _gameRepository.GetLatest(3),
                LatestBlogPosts = _blogPostRepository.GetLatest(2)
            };

            return View(viewModel);
        }

        [Route("/Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Games")]
        public IActionResult Games()
        {
            return View(_gameRepository.GetAll());
        }

        [Route("/Services")]
        public IActionResult Services()
        {
            return View(ServiceSeedData.All);
        }

        [Route("/About")]
        public IActionResult About()
        {
            return View();
        }
    }
}
