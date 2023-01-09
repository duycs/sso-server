using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AuthServer.Extensions;

namespace AuthServer.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // if (_environment.IsDevelopment())
            // {
            // only show in development
            return View();
            // }

            //_logger.LogInformation("Homepage is disabled in production. Returning 404.");
            //return NotFound();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
