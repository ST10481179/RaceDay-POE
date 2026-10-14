using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.WebAPI.Data;
using RaceDay.WebAPI.Models;

namespace RaceDay.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("me")]
    public Task<ActionResult<object>> GetMe() => GetProfile(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id}")]
    public Task<ActionResult<object>> Get(string id) => GetProfile(id);

    [HttpPut("me")]
    public Task<ActionResult<object>> UpdateMe(UpdateProfileRequest request) =>
        UpdateProfile(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request);

    [HttpPut("{id}")]
    public Task<ActionResult<object>> Update(string id, UpdateProfileRequest request) => UpdateProfile(id, request);

    private async Task<ActionResult<object>> GetProfile(string id)
    {
        if (!IsCurrentUser(id))
        {
            return Forbid();
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new { user.Id, user.UserName, user.Email, user.FirstName, user.LastName, user.Age, Role = roles.FirstOrDefault() });
    }

    private async Task<ActionResult<object>> UpdateProfile(string id, UpdateProfileRequest request)
    {
        if (!IsCurrentUser(id))
        {
            return Forbid();
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var previousEmail = user.Email;
        var participant = await dbContext.Participants.SingleOrDefaultAsync(value => value.Email == previousEmail);
        if (participant is not null && !string.Equals(previousEmail, request.Email, StringComparison.OrdinalIgnoreCase) &&
            await dbContext.Participants.AnyAsync(value => value.Email == request.Email))
        {
            return Conflict(new { error = "A participant with that email already exists." });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email;
        user.UserName = request.Email;
        user.Age = request.Age;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new { errors = updateResult.Errors.Select(error => error.Description) });
        }

        if (participant is not null)
        {
            participant.Email = user.Email!;
            participant.FirstName = user.FirstName;
            participant.LastName = user.LastName;
            if (user.Age.HasValue)
            {
                participant.Age = user.Age.Value;
            }

            await dbContext.SaveChangesAsync();
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new { user.Id, user.UserName, user.Email, user.FirstName, user.LastName, user.Age, Role = roles.FirstOrDefault() });
    }

    private bool IsCurrentUser(string id) =>
        string.Equals(id, User.FindFirstValue(ClaimTypes.NameIdentifier), StringComparison.Ordinal);
}