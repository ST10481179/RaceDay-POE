using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IProfileImageService, ProfileImageService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // Apply any pending migrations to the

    var rolmanager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Organizer", "Participant" };
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    
    foreach (var role in roles)
    {
        if (!await rolmanager.RoleExistsAsync(role))
        {
            await rolmanager.CreateAsync(new IdentityRole(role));
        }
    }

    // If an older "User" role exists, migrate its members to "Organizer" and remove the old role.
    if (await rolmanager.RoleExistsAsync("User"))
    {
        // ensure Organizer exists
        if (!await rolmanager.RoleExistsAsync("Organizer"))
        {
            await rolmanager.CreateAsync(new IdentityRole("Organizer"));
        }

        var usersInOldRole = await userManager.GetUsersInRoleAsync("User");
        foreach (var u in usersInOldRole)
        {
            // add to Organizer and remove from User
            if (!await userManager.IsInRoleAsync(u, "Organizer"))
            {
                await userManager.AddToRoleAsync(u, "Organizer");
            }
            await userManager.RemoveFromRoleAsync(u, "User");
        }

        var oldRole = await rolmanager.FindByNameAsync("User");
        if (oldRole != null)
        {
            await rolmanager.DeleteAsync(oldRole);
        }
    }
    var useerManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    string email = "admin@example.com";
    string password = "Admin@123"; // Ensure this meets your password

    if(await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser();

        user.UserName = email;
        user.Email = email;
        user.EmailConfirmed = true; // Set to true if you want to bypass email confirmation

        await userManager.CreateAsync(user,password);
        await userManager.AddToRoleAsync(user, "Admin");
    }

  }




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
