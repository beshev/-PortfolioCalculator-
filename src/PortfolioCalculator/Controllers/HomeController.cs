namespace PortfolioCalculator.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PortfolioCalculator.Models;
    using System.Diagnostics;

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.RefreshTimeInMilliseconds = _configuration.GetValue<int>("Portfolio:RefreshTimeInMilliseconds");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
