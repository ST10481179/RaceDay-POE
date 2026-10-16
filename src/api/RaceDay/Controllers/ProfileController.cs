using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Data;

namespace RaceDay.Controllers
{
    [Authorize]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Keep explicit constructor for clarity and compatibility")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,FirstName,LastName,Gender,ProfileImagePath")] ApplicationUser input)
        {
            var user = await _userManager.FindByIdAsync(input.Id);
            if (user == null) return NotFound();
            user.FirstName = input.FirstName;
            user.LastName = input.LastName;
            user.Gender = input.Gender;
            user.ProfileImagePath = input.ProfileImagePath;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                return View(input);
            }
            return RedirectToAction("Index","Home");
        }
    }
}
