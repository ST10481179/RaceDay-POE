using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using RaceDay.Data;
using RaceDay.// RoleHelper.cs
using Microsoft.AspNetCore.Identity;
using RaceDay.Data;
using System.Threading.Tasks;

namespace RaceDay.Utils
{
    public static class RoleHelper
    {
        public enum Destination
        {
            None,
            Admin,
            Participant,
            Organiser
        }

        public static async Task<Destination> DetermineDestinationAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            if (userManager == null || user == null) return Destination.None;

            if (await userManager.IsInRoleAsync(user, "Admin")) return Destination.Admin;
            if (await userManager.IsInRoleAsync(user, "Participant")) return Destination.Participant;
            if (await userManager.IsInRoleAsync(user, "Organiser")) return Destination.Organiser;

            return Destination.None;
        }
    }
};
using System.Threading.Tasks;
using Xunit;

namespace RaceDay.Tests
{
    public class RoleHelperTests
    {
        private ApplicationUser MakeUser(string email) =>
            new ApplicationUser { Email = email, UserName = email };

        [Fact]
        public async Task Returns_Admin_When_In_Admin_Role()
        {
            var user = MakeUser("admin@example.com");

            // Fake UserManager with only IsInRoleAsync implemented
            var userManager = new FakeUserManager("Admin");

            var dest = await RoleHelper.DetermineDestinationAsync(userManager, user);

            Assert.Equal(RoleHelper.Destination.Admin, dest);
        }

        [Fact]
        public async Task Returns_Participant_When_In_Participant_Role()
        {
            var user = MakeUser("part@example.com");
            var userManager = new FakeUserManager("Participant");

            var dest = await RoleHelper.DetermineDestinationAsync(userManager, user);

            Assert.Equal(RoleHelper.Destination.Participant, dest);
        }

        [Fact]
        public async Task Returns_Organiser_When_In_Organiser_Role()
        {
            var user = MakeUser("org@example.com");
            var userManager = new FakeUserManager("Organiser");

            var dest = await RoleHelper.DetermineDestinationAsync(userManager, user);

            Assert.Equal(RoleHelper.Destination.Organiser, dest);
        }

        [Fact]
        public async Task Returns_None_When_No_Roles()
        {
            var user = MakeUser("none@example.com");
            var userManager = new FakeUserManager(); // no roles

            var dest = await RoleHelper.DetermineDestinationAsync(userManager, user);

            Assert.Equal(RoleHelper.Destination.None, dest);
        }
    }

    // Simple fake UserManager that only cares about IsInRoleAsync
    public class FakeUserManager : UserManager<ApplicationUser>
    {
        private readonly string _role;

        public FakeUserManager(string role = null)
            : base(new FakeUserStore(),
                   null, null, null, null, null, null, null, null)
        {
            _role = role;
        }

        public override Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            return Task.FromResult(_role == role);
        }
    }

    public class FakeUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.Id);
        public Task<string> GetUserNameAsync(ApplicationUser user, System.Threading.CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public