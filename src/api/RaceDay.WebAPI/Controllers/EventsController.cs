using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.WebAPI.Data;
using RaceDay.WebAPI.Models;

namespace RaceDay.WebAPI.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await dbContext.Events.AsNoTracking()
            .OrderBy(value => value.EventDate)
            .Select(value => new
            {
                value.EventID,
                value.EventName,
                value.EventDate,
                value.Location,
                value.SponsorID
            })
            .ToListAsync();
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var eventDetails = await dbContext.Events.AsNoTracking()
            .Where(value => value.EventID == id)
            .Select(value => new
            {
                value.EventID,
                value.EventName,
                value.EventDate,
                value.Location,
                value.SponsorID
            })
            .SingleOrDefaultAsync();
        return eventDetails is null ? NotFound() : Ok(eventDetails);
    }

    [Authorize(Policy = "OrganiserOnly")]
    [HttpPost]
    public async Task<IActionResult> Create(EventRequest request)
    {
        if (request.EventDate == default)
        {
            return BadRequest(new { error = "EventDate is required." });
        }

        if (request.SponsorID.HasValue && !await dbContext.Sponsors.AnyAsync(value => value.SponsorID == request.SponsorID))
        {
            return BadRequest(new { error = "The selected sponsor does not exist." });
        }

        var raceEvent = new Event
        {
            EventName = request.EventName.Trim(),
            EventDate = request.EventDate.Date,
            Location = request.Location.Trim(),
            SponsorID = request.SponsorID
        };
        dbContext.Events.Add(raceEvent);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = raceEvent.EventID }, ToResponse(raceEvent));
    }

    [Authorize(Policy = "OrganiserOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var raceEvent = await dbContext.Events.FindAsync(id);
        if (raceEvent is null)
        {
            return NotFound();
        }

        if (request.EventDate == default)
        {
            return BadRequest(new { error = "EventDate is required." });
        }

        if (request.SponsorID.HasValue && !await dbContext.Sponsors.AnyAsync(value => value.SponsorID == request.SponsorID))
        {
            return BadRequest(new { error = "The selected sponsor does not exist." });
        }

        raceEvent.EventName = request.EventName.Trim();
        raceEvent.EventDate = request.EventDate.Date;
        raceEvent.Location = request.Location.Trim();
        raceEvent.SponsorID = request.SponsorID;
        await dbContext.SaveChangesAsync();
        return Ok(ToResponse(raceEvent));
    }

    [Authorize(Policy = "OrganiserOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var raceEvent = await dbContext.Events.FindAsync(id);
        if (raceEvent is null)
        {
            return NotFound();
        }

        if (await dbContext.Enrollments.AnyAsync(value => value.EventID == id) ||
            await dbContext.Categories.AnyAsync(value => value.EventID == id))
        {
            return Conflict(new { error = "Events with categories or enrolments cannot be deleted." });
        }

        dbContext.Events.Remove(raceEvent);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static object ToResponse(Event value) => new
    {
        value.EventID,
        value.EventName,
        value.EventDate,
        value.Location,
        value.SponsorID
    };
}