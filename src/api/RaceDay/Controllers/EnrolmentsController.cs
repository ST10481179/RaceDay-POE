using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [Authorize]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Keep explicit constructor for clarity and compatibility")]
    public class EnrolmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrolmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Participants create enrolment
        public async Task<IActionResult> Create(int eventId)
        {
            if (_context.Categories == null) return Problem("Entity set 'ApplicationDbContext.Categories' is null.");
            var categories = await _context.Categories.Where(c => c.EventId == eventId).ToListAsync();
            ViewBag.EventId = eventId;
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId, int categoryId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Forbid();
            var enrol = new Enrolment { EventId = eventId, CategoryId = categoryId, ParticipantId = userId };
            _context.Add(enrol);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Events");
        }

        // Organiser: view enrolments for their event
        [Authorize(Roles = "Organiser,Admin")]
        public async Task<IActionResult> Index(int eventId)
        {
            if (_context.Events == null) return Problem("Entity set 'ApplicationDbContext.Events' is null.");
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return NotFound();
            if (@event.OrganiserId != _userManager.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();
            if (_context.Enrolments == null) return Problem("Entity set 'ApplicationDbContext.Enrolments' is null.");
            var enrolments = await _context.Enrolments.Include(e => e.Participant).Include(e => e.Category).Where(e => e.EventId == eventId).ToListAsync();
            ViewBag.Event = @event;
            return View(enrolments);
        }

        // Participants can view their enrolments
        public async Task<IActionResult> MyEnrolments()
        {
            var userId = _userManager.GetUserId(User);
            if (_context.Enrolments == null) return Problem("Entity set 'ApplicationDbContext.Enrolments' is null.");
            var enrolments = await _context.Enrolments.Include(e => e.Event).Include(e => e.Category).Where(e => e.ParticipantId == userId).ToListAsync();
            return View(enrolments);
        }
    }
}
