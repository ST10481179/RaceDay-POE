using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.WebAPI.Data;
using RaceDay.WebAPI.Models;

namespace RaceDay.WebAPI.Controllers;

[ApiController]
[Route("api")]
public sealed class EnrolmentsController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [Authorize(Policy = "ParticipantOnly")]
    [HttpPost("events/{eventId:int}/enrol")]
    public async Task<IActionResult> Enrol(int eventId, EnrolRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user?.Email is null)
        {
            return Unauthorized();
        }

        var participant = await dbContext.Participants.SingleOrDefaultAsync(value => value.Email == user.Email);
        if (participant is null)
        {
            return Conflict(new { error = "The participant profile has not been initialized." });
        }

        var categoryExists = await dbContext.Categories.AnyAsync(value =>
            value.CategoryID == request.CategoryID && value.EventID == eventId);
        if (!categoryExists)
        {
            return NotFound(new { error = "The category was not found for this event." });
        }

        if (await dbContext.Enrollments.AnyAsync(value =>
                value.ParticipantID == participant.ParticipantID && value.EventID == eventId))
        {
            return Conflict(new { error = "The participant is already enrolled in this event." });
        }

        var enrollment = new Enrollment
        {
            ParticipantID = participant.ParticipantID,
            CategoryID = request.CategoryID,
            EventID = eventId,
            Status = "Registered"
        };
        dbContext.Enrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();
        return Created($"/api/users/{user.Id}/enrolments", ToResponse(enrollment));
    }

    [Authorize(Policy = "OrganiserOnly")]
    [HttpGet("events/{eventId:int}/enrolments")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await dbContext.Events.AnyAsync(value => value.EventID == eventId))
        {
            return NotFound();
        }

        var enrollments = await dbContext.Enrollments.AsNoTracking()
            .Where(value => value.EventID == eventId)
            .Select(value => new
            {
                value.EnrollmentID,
                value.EventID,
                value.CategoryID,
                value.ParticipantID,
                ParticipantName = value.Participant!.FirstName + " " + value.Participant.LastName,
                ParticipantEmail = value.Participant!.Email,
                value.Status
            })
            .ToListAsync();
        return Ok(enrollments);
    }

    [Authorize(Policy = "ParticipantOnly")]
    [HttpGet("users/{userId}/enrolments")]
    public async Task<IActionResult> GetForUser(string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.Equals(userId, currentUserId, StringComparison.Ordinal))
        {
            return Forbid();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user?.Email is null)
        {
            return NotFound();
        }

        var enrollments = await dbContext.Enrollments.AsNoTracking()
            .Where(value => value.Participant!.Email == user.Email)
            .Select(value => new
            {
                value.EnrollmentID,
                value.EventID,
                EventName = value.Event!.EventName,
                value.CategoryID,
                CategoryName = value.Category!.CategoryName,
                value.Status
            })
            .ToListAsync();
        return Ok(enrollments);
    }

    private static object ToResponse(Enrollment value) => new
    {
        value.EnrollmentID,
        value.EventID,
        value.CategoryID,
        value.ParticipantID,
        value.Status
    };
}