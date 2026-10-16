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
    public class ResultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ResultsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Organiser adds results for an event
        [Authorize(Roles = "Organiser,Admin")]
        public async Task<IActionResult> Manage(int eventId)
        {
            if (_context.Events == null) return Problem("Entity set 'ApplicationDbContext.Events' is null.");
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return NotFound();
            if (@event.OrganiserId != _userManager.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();

            if (_context.Enrolments == null) return Problem("Entity set 'ApplicationDbContext.Enrolments' is null.");
            var enrolments = await _context.Enrolments.Include(e => e.Participant).Where(e => e.EventId == eventId).ToListAsync();
            ViewBag.Event = @event;
            return View(enrolments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organiser,Admin")]
        public async Task<IActionResult> UpdateResult(int eventId, int participantId, TimeSpan? finish, int? position)
        {
            if (_context.Events == null) return Problem("Entity set 'ApplicationDbContext.Events' is null.");
            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return NotFound();
            if (@event.OrganiserId != _userManager.GetUserId(User) && !User.IsInRole("Admin")) return Forbid();

            if (_context.Enrolments == null) return Problem("Entity set 'ApplicationDbContext.Enrolments' is null.");
            var enrol = await _context.Enrolments.Include(e => e.Participant).FirstOrDefaultAsync(e => e.EventId == eventId && e.ParticipantId == participantId.ToString());
            if (enrol == null) return NotFound();

            if (_context.Results == null) return Problem("Entity set 'ApplicationDbContext.Results' is null.");
            var result = await _context.Results.FirstOrDefaultAsync(r => r.EventId == eventId && r.ParticipantId == enrol.ParticipantId);
            if (result == null)
            {
                result = new Result { EventId = eventId, ParticipantId = enrol.ParticipantId, FinishTime = finish, Position = position };
                _context.Results!.Add(result);
            }
            else
            {
                result.FinishTime = finish;
                result.Position = position;
                _context.Results.Update(result);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Manage", new { eventId });
        }

        // Participants view their results
        public async Task<IActionResult> MyResults()
        {
            var userId = _userManager.GetUserId(User);
            if (_context.Results == null) return Problem("Entity set 'ApplicationDbContext.Results' is null.");
            var results = await _context.Results.Include(r => r.Event).Where(r => r.ParticipantId == userId).ToListAsync();
            return View(results);
        }
    }
}
