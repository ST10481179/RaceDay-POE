using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Controllers;

[Authorize(Roles = "Admin,Organiser,Participant")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Participant"))
            return View("~/Views/Home/ParticipantDashboard.cshtml");

        return View("~/Views/Home/OrganiserDashboard.cshtml");
    }
}
