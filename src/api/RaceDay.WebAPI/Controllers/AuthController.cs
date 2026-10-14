using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.WebAPI.Data;
using RaceDay.WebAPI.Models;

namespace RaceDay.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext dbContext,
    IConfiguration configuration) : ControllerBase
{
    private static readonly string[] AllowedRoles = ["Organiser", "Participant"];

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var role = AllowedRoles.FirstOrDefault(value =>
            string.Equals(value, request.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null)
        {
            return BadRequest(new { error = "Role must be Organiser or Participant." });
        }

        if (role == "Participant" && request.Age is null)
        {
            return BadRequest(new { error = "Age is required for participants." });
        }

        var (firstName, lastName) = ResolveName(request);
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return BadRequest(new { error = "Provide FirstName and LastName, or a FullName containing both." });
        }

        var userName = string.IsNullOrWhiteSpace(request.UserName) ? request.Email : request.UserName.Trim();
        if (await userManager.FindByEmailAsync(request.Email) is not null ||
            await userManager.FindByNameAsync(userName) is not null)
        {
            return Conflict(new { error = "An account with that email or username already exists." });
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                return Problem("The requested role could not be initialized.");
            }
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = request.Email,
            FirstName = firstName,
            LastName = lastName,
            Age = request.Age
        };
        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new { errors = createResult.Errors.Select(error => error.Description) });
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Problem("The account role could not be assigned.");
        }

        if (role == "Participant")
        {
            var participant = await dbContext.Participants.SingleOrDefaultAsync(
                value => value.Email == request.Email);
            if (participant is null)
            {
                participant = new Participant { Email = request.Email };
                dbContext.Participants.Add(participant);
            }

            participant.FirstName = user.FirstName;
            participant.LastName = user.LastName;
            participant.Age = request.Age!.Value;
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                await userManager.DeleteAsync(user);
                return Conflict(new { error = "A participant with that email already exists." });
            }
        }

        return StatusCode(StatusCodes.Status201Created, await CreateAuthResponse(user, role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var identifier = string.IsNullOrWhiteSpace(request.UserName) ? request.Email : request.UserName;
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return BadRequest(new { error = "Provide a username or email address." });
        }

        var user = await userManager.FindByNameAsync(identifier)
            ?? await userManager.FindByEmailAsync(identifier);
        var signInResult = user is null
            ? null
            : await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (user is null || signInResult is null || !signInResult.Succeeded)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            return Unauthorized(new { error = "The account has no assigned role." });
        }

        return Ok(await CreateAuthResponse(user, roles[0]));
    }

    private async Task<AuthResponse> CreateAuthResponse(ApplicationUser user, string role)
    {
        var expiresAt = DateTime.UtcNow.AddHours(2);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims,
            expires: expiresAt, signingCredentials: credentials);

        return new AuthResponse(user.Id, user.UserName ?? string.Empty, role,
            new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static (string FirstName, string LastName) ResolveName(RegisterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName))
        {
            return (request.FirstName.Trim(), request.LastName.Trim());
        }

        var parts = request.FullName?.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts is { Length: 2 } ? (parts[0], parts[1]) : (string.Empty, string.Empty);
    }
}