using Microsoft.AspNetCore.Mvc;
using DemoProject.Models;
using System.Diagnostics;

namespace DemoProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult OrganiserDashboard()
        {
            return View();
        }
        public IActionResult Index()
        {
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
