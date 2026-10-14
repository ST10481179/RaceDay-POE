using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.WebAPI.Data;
using RaceDay.WebAPI.Models;

namespace RaceDay.WebAPI.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
public sealed class CategoriesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await dbContext.Events.AnyAsync(value => value.EventID == eventId))
        {
            return NotFound();
        }

        var categories = await dbContext.Categories.AsNoTracking()
            .Where(value => value.EventID == eventId)
            .Select(value => new { value.CategoryID, value.CategoryName, value.Distance, value.AgeRange, value.EventID })
            .ToListAsync();
        return Ok(categories);
    }

    [Authorize(Policy = "OrganiserOnly")]
    [HttpPost]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request)
    {
        if (!await dbContext.Events.AnyAsync(value => value.EventID == eventId))
        {
            return NotFound();
        }

        var category = new Category
        {
            CategoryName = request.CategoryName.Trim(),
            Distance = request.Distance,
            AgeRange = request.AgeRange?.Trim(),
            EventID = eventId
        };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetForEvent), new { eventId }, ToResponse(category));
    }

    private static object ToResponse(Category value) => new
    {
        value.CategoryID,
        value.CategoryName,
        value.Distance,
        value.AgeRange,
        value.EventID
    };
}