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
public sealed class ResultsController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [Authorize(Policy = "OrganiserOnly")]
    [HttpPost("events/{eventId:int}/results")]
    public async Task<IActionResult> Create(int eventId, ResultRequest request)
    {
        if (request.FinishTime <= TimeSpan.Zero)
        {
            return BadRequest(new { error = "FinishTime must be greater than zero." });
        }

        var enrollment = await dbContext.Enrollments.SingleOrDefaultAsync(value =>
            value.EventID == eventId && value.ParticipantID == request.ParticipantID);
        if (enrollment is null)
        {
            return NotFound(new { error = "No enrolment exists for this participant and event." });
        }

        if (await dbContext.Results.AnyAsync(value => value.EnrollmentID == enrollment.EnrollmentID))
        {
            return Conflict(new { error = "A result has already been recorded for this enrolment." });
        }

        var result = new Result
        {
            EnrollmentID = enrollment.EnrollmentID,
            FinishTime = request.FinishTime,
            Position = request.Position
        };
        dbContext.Results.Add(result);
        await dbContext.SaveChangesAsync();
        return Created($"/api/events/{eventId}/results", ToResponse(result, request.ParticipantID));
    }

    [HttpGet("events/{eventId:int}/results")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await dbContext.Events.AnyAsync(value => value.EventID == eventId))
        {
            return NotFound();
        }

        var results = await dbContext.Results.AsNoTracking()
            .Where(value => value.Enrollment!.EventID == eventId)
            .Select(value => new
            {
                value.ResultID,
                value.FinishTime,
                value.Position,
                value.EnrollmentID,
                value.Enrollment!.ParticipantID
            })
            .ToListAsync();
        return Ok(results);
    }

    [Authorize(Policy = "ParticipantOnly")]
    [HttpGet("users/{userId}/results")]
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

        var results = await dbContext.Results.AsNoTracking()
            .Where(value => value.Enrollment!.Participant!.Email == user.Email)
            .Select(value => new
            {
                value.ResultID,
                value.FinishTime,
                value.Position,
                value.EnrollmentID,
                EventName = value.Enrollment!.Event!.EventName,
                CategoryName = value.Enrollment.Category!.CategoryName
            })
            .ToListAsync();
        return Ok(results);
    }

    private static object ToResponse(Result value, int participantId) => new
    {
        value.ResultID,
        value.EnrollmentID,
        ParticipantID = participantId,
        value.FinishTime,
        value.Position
    };
}