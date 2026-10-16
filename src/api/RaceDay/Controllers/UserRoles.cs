using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Models.ViewModels;
using RaceDay.Data;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace RaceDay.Controllers
{
    [Authorize(Roles = "Admin")]
    [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Keep explicit constructor for compatibility")]
    public class UserRoles : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserRoles(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // GET: /UserRoles
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (var user in users)
            {
                var thisViewModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Roles = await _userManager.GetRolesAsync(user)
                };
                userRolesViewModel.Add(thisViewModel);
            }
            return View(userRolesViewModel);
        }


        // GET: /UserRoles/Edit/{userId}
        public async Task<IActionResult> Manage(string id)
        {
            ViewBag.userId = id;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }
            ViewBag.Username = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles.ToList())
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? "",
                    isSelected = await _userManager.IsInRoleAsync(user, role.Name ?? "")
                };
                if (await _userManager.IsInRoleAsync(user, role.Name ?? ""))
                {
                    userRolesViewModel.isSelected = true;
                }
                else
                {
                    userRolesViewModel.isSelected = false;
                }
                model.Add(userRolesViewModel);
            }
            ;
            return View(model);
        }
        // POST: /UserRoles/Edit/{userId}
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.isSelected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }
    }

}   


        