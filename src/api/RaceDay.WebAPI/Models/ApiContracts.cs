using System.ComponentModel.DataAnnotations;

namespace RaceDay.WebAPI.Models;

public sealed class RegisterRequest
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(256)]
    public string? UserName { get; init; }

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    public string? Role { get; init; }

    [MaxLength(100)]
    public string? FirstName { get; init; }

    [MaxLength(100)]
    public string? LastName { get; init; }

    [MaxLength(201)]
    public string? FullName { get; init; }

    [Range(1, 120)]
    public int? Age { get; init; }
}

public sealed class LoginRequest
{
    [MaxLength(256)]
    public string? UserName { get; init; }

    [EmailAddress, MaxLength(100)]
    public string? Email { get; init; }

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed record AuthResponse(string UserId, string UserName, string Role, string Token, DateTime ExpiresAt);

public sealed class UpdateProfileRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    [Range(1, 120)]
    public int? Age { get; init; }
}

public sealed class EventRequest
{
    [Required, MaxLength(100)]
    public string EventName { get; init; } = string.Empty;

    public DateTime EventDate { get; init; }

    [Required, MaxLength(150)]
    public string Location { get; init; } = string.Empty;

    public int? SponsorID { get; init; }
}

public sealed class CategoryRequest
{
    [Required, MaxLength(100)]
    public string CategoryName { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999.99")]
    public decimal Distance { get; init; }

    [MaxLength(50)]
    public string? AgeRange { get; init; }
}

public sealed class EnrolRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryID { get; init; }
}

public sealed class ResultRequest
{
    [Range(1, int.MaxValue)]
    public int ParticipantID { get; init; }

    public TimeSpan FinishTime { get; init; }

    [Range(1, int.MaxValue)]
    public int Position { get; init; }
}