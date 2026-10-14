using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.WebAPI.Data;
using Xunit;

namespace RaceDay.WebAPI.Tests;

public sealed class RaceDayApiTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory factory;
    private readonly HttpClient client;

    public RaceDayApiTests(RaceDayApiFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterAndLogin_ReturnToken_AndRejectInvalidCredentialsAndRoles()
    {
        var email = NewEmail();
        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "RaceDay1234!",
            role = "Participant",
            fullName = "Ava Runner",
            age = 24
        });

        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        using var registrationBody = JsonDocument.Parse(await registration.Content.ReadAsStringAsync());
        Assert.Equal("Participant", registrationBody.RootElement.GetProperty("role").GetString());
        Assert.False(string.IsNullOrWhiteSpace(registrationBody.RootElement.GetProperty("token").GetString()));

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "RaceDay1234!"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var invalidLogin = await client.PostAsJsonAsync("/api/auth/login", new
        {
            userName = email,
            password = "WrongPassword1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidLogin.StatusCode);

        var invalidRole = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = NewEmail(),
            password = "RaceDay1234",
            role = "Admin",
            firstName = "Ava",
            lastName = "Runner"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidRole.StatusCode);

        var swagger = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
    }

    [Fact]
    public async Task EventManagement_IsRestrictedToOrganisers()
    {
        var participant = await Register("Participant");
        var organiser = await Register("Organiser");
        var eventBody = new { eventName = "Durban Run", eventDate = "2026-11-05", location = "Durban" };

        using var participantRequest = Authorized(HttpMethod.Post, "/api/events", participant.Token, eventBody);
        var participantResponse = await client.SendAsync(participantRequest);
        Assert.Equal(HttpStatusCode.Forbidden, participantResponse.StatusCode);

        using var organiserRequest = Authorized(HttpMethod.Post, "/api/events", organiser.Token, eventBody);
        var organiserResponse = await client.SendAsync(organiserRequest);
        Assert.Equal(HttpStatusCode.Created, organiserResponse.StatusCode);
        using var createdBody = JsonDocument.Parse(await organiserResponse.Content.ReadAsStringAsync());
        var eventId = createdBody.RootElement.GetProperty("eventID").GetInt32();

        using var participantUpdate = Authorized(HttpMethod.Put, $"/api/events/{eventId}", participant.Token,
            new { eventName = "Updated Run", eventDate = "2026-11-06", location = "Durban" });
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(participantUpdate)).StatusCode);

        using var organiserUpdate = Authorized(HttpMethod.Put, $"/api/events/{eventId}", organiser.Token,
            new { eventName = "Updated Run", eventDate = "2026-11-06", location = "Durban" });
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(organiserUpdate)).StatusCode);

        var publicRead = await client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, publicRead.StatusCode);

        using var participantDelete = Authorized(HttpMethod.Delete, $"/api/events/{eventId}", participant.Token);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(participantDelete)).StatusCode);

        using var organiserDelete = Authorized(HttpMethod.Delete, $"/api/events/{eventId}", organiser.Token);
        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(organiserDelete)).StatusCode);
    }

    [Fact]
    public async Task Enrolment_IsParticipantOnly_AndOrganisersCanViewEventEnrolments()
    {
        var participant = await Register("Participant");
        var organiser = await Register("Organiser");
        var (eventId, categoryId) = await CreateEventAndCategory(organiser.Token);

        using var enrolRequest = Authorized(HttpMethod.Post, $"/api/events/{eventId}/enrol", participant.Token,
            new { categoryID = categoryId });
        var enrolResponse = await client.SendAsync(enrolRequest);
        Assert.Equal(HttpStatusCode.Created, enrolResponse.StatusCode);

        using var duplicateRequest = Authorized(HttpMethod.Post, $"/api/events/{eventId}/enrol", participant.Token,
            new { categoryID = categoryId });
        var duplicateResponse = await client.SendAsync(duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        using var ownEnrolmentsRequest = Authorized(HttpMethod.Get,
            $"/api/users/{participant.UserId}/enrolments", participant.Token);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(ownEnrolmentsRequest)).StatusCode);

        using var participantViewRequest = Authorized(HttpMethod.Get,
            $"/api/events/{eventId}/enrolments", participant.Token);
        var participantView = await client.SendAsync(participantViewRequest);
        Assert.Equal(HttpStatusCode.Forbidden, participantView.StatusCode);

        using var organiserViewRequest = Authorized(HttpMethod.Get,
            $"/api/events/{eventId}/enrolments", organiser.Token);
        var organiserView = await client.SendAsync(organiserViewRequest);
        Assert.Equal(HttpStatusCode.OK, organiserView.StatusCode);
    }

    [Fact]
    public async Task Results_AreCapturedByOrganisers_AndVisibleToParticipants()
    {
        var participant = await Register("Participant");
        var organiser = await Register("Organiser");
        var (eventId, categoryId) = await CreateEventAndCategory(organiser.Token);

        using var enrolRequest = Authorized(HttpMethod.Post, $"/api/events/{eventId}/enrol", participant.Token,
            new { categoryID = categoryId });
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(enrolRequest)).StatusCode);

        using var resultRequest = Authorized(HttpMethod.Post, $"/api/events/{eventId}/results", organiser.Token,
            new { participantID = participant.ParticipantId, finishTime = "00:45:12", position = 1 });
        var resultResponse = await client.SendAsync(resultRequest);
        Assert.Equal(HttpStatusCode.Created, resultResponse.StatusCode);

        using var participantResultsRequest = Authorized(HttpMethod.Get,
            $"/api/users/{participant.UserId}/results", participant.Token);
        var participantResults = await client.SendAsync(participantResultsRequest);
        Assert.Equal(HttpStatusCode.OK, participantResults.StatusCode);
        using var resultBody = JsonDocument.Parse(await participantResults.Content.ReadAsStringAsync());
        Assert.Single(resultBody.RootElement.EnumerateArray());

        using var forbiddenResultRequest = Authorized(HttpMethod.Post,
            $"/api/events/{eventId}/results", participant.Token,
            new { participantID = participant.ParticipantId, finishTime = "00:45:12", position = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(forbiddenResultRequest)).StatusCode);

        using var organiserResultsRequest = Authorized(HttpMethod.Get,
            $"/api/users/{participant.UserId}/results", organiser.Token);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(organiserResultsRequest)).StatusCode);
    }

    [Fact]
    public async Task ParticipantCanViewAndUpdateOwnProfile()
    {
        var participant = await Register("Participant");
        var otherParticipant = await Register("Participant");
        using var getRequest = Authorized(HttpMethod.Get, "/api/users/me", participant.Token);
        var getResponse = await client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var otherProfileRequest = Authorized(HttpMethod.Get,
            $"/api/users/{otherParticipant.UserId}", participant.Token);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.SendAsync(otherProfileRequest)).StatusCode);

        var updatedEmail = NewEmail();
        using var updateRequest = Authorized(HttpMethod.Put, "/api/users/me", participant.Token,
            new { firstName = "Updated", lastName = "Runner", email = updatedEmail, age = 31 });
        var updateResponse = await client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        using var updateBody = JsonDocument.Parse(await updateResponse.Content.ReadAsStringAsync());
        Assert.Equal("Updated", updateBody.RootElement.GetProperty("firstName").GetString());

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedParticipant = await dbContext.Participants.SingleAsync(value => value.Email == updatedEmail);
        Assert.Equal("Updated", updatedParticipant.FirstName);
        Assert.Equal(31, updatedParticipant.Age);
    }

    private async Task<(string UserId, string Token, int ParticipantId)> Register(string role)
    {
        var email = NewEmail();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "RaceDay1234!",
            role,
            firstName = "Casey",
            lastName = "Athlete",
            age = 30
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var userId = body.RootElement.GetProperty("userId").GetString()!;
        var token = body.RootElement.GetProperty("token").GetString()!;

        var participantId = 0;
        if (role == "Participant")
        {
            using var scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            participantId = await dbContext.Participants
                .Where(value => value.Email == email)
                .Select(value => value.ParticipantID)
                .SingleAsync();
        }

        return (userId, token, participantId);
    }

    private async Task<(int EventId, int CategoryId)> CreateEventAndCategory(string token)
    {
        using var eventRequest = Authorized(HttpMethod.Post, "/api/events", token,
            new { eventName = "Test Race", eventDate = "2026-11-05", location = "Cape Town" });
        var eventResponse = await client.SendAsync(eventRequest);
        Assert.Equal(HttpStatusCode.Created, eventResponse.StatusCode);
        using var eventBody = JsonDocument.Parse(await eventResponse.Content.ReadAsStringAsync());
        var eventId = eventBody.RootElement.GetProperty("eventID").GetInt32();

        using var categoryRequest = Authorized(HttpMethod.Post, $"/api/events/{eventId}/categories", token,
            new { categoryName = "10km Run", distance = 10.0m, ageRange = "Open" });
        var categoryResponse = await client.SendAsync(categoryRequest);
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        using var categoryBody = JsonDocument.Parse(await categoryResponse.Content.ReadAsStringAsync());
        var categoryId = categoryBody.RootElement.GetProperty("categoryID").GetInt32();
        return (eventId, categoryId);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string uri, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private static string NewEmail() => $"{Guid.NewGuid():N}@raceday.test";
}