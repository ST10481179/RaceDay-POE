using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using RaceDay.Data;
using RaceDay.Utils;

namespace RaceDay.Tests
{
    public class RoleHelperTests
    {
        private static UserManager<ApplicationUser> CreateUserManagerMock(Mock<IUserStore<ApplicationUser>> storeMock)
        {
            var mgr = new UserManager<ApplicationUser>(
                storeMock.Object,
                null, null, null, null, null, null, null, null);
            return mgr;
        }

        [Fact]
        public async Task DetermineDestinationAsync_Returns_Admin_When_In_Admin_Role()
        {
            var user = new ApplicationUser { Email = "admin@example.com" };

            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            var userManager = CreateUserManagerMock(storeMock);

            var umMock = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null, null, null, null, null, null, null, null);
            umMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);

            var dest = await RoleHelper.DetermineDestinationAsync(umMock.Object, user);

            Assert.Equal(RoleHelper.Destination.Admin, dest);
        }

        [Fact]
        public async Task DetermineDestinationAsync_Returns_Participant_When_In_Participant_Role()
        {
            var user = new ApplicationUser { Email = "part@example.com" };
            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            var umMock = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null, null, null, null, null, null, null, null);
            umMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            umMock.Setup(x => x.IsInRoleAsync(user, "Participant")).ReturnsAsync(true);

            var dest = await RoleHelper.DetermineDestinationAsync(umMock.Object, user);

            Assert.Equal(RoleHelper.Destination.Participant, dest);
        }

        [Fact]
        public async Task DetermineDestinationAsync_Returns_Organiser_When_In_Organiser_Role()
        {
            var user = new ApplicationUser { Email = "org@example.com" };
            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            var umMock = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null, null, null, null, null, null, null, null);
            umMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            umMock.Setup(x => x.IsInRoleAsync(user, "Participant")).ReturnsAsync(false);
            umMock.Setup(x => x.IsInRoleAsync(user, "Organiser")).ReturnsAsync(true);

            var dest = await RoleHelper.DetermineDestinationAsync(umMock.Object, user);

            Assert.Equal(RoleHelper.Destination.Organiser, dest);
        }

        [Fact]
        public async Task DetermineDestinationAsync_Returns_None_When_No_Roles()
        {
            var user = new ApplicationUser { Email = "none@example.com" };
            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            var umMock = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null, null, null, null, null, null, null, null);
            umMock.Setup(x => x.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            umMock.Setup(x => x.IsInRoleAsync(user, "Participant")).ReturnsAsync(false);
            umMock.Setup(x => x.IsInRoleAsync(user, "Organiser")).ReturnsAsync(false);

            var dest = await RoleHelper.DetermineDestinationAsync(umMock.Object, user);

            Assert.Equal(RoleHelper.Destination.None, dest);
        }
    }
}
