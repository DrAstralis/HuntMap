using HuntMap.Data;
using HuntMap.Domain;
using HuntMap.Web;                    // <-- add this (for PinHub)
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI; // optional if you later add Identity UI

var builder = WebApplication.CreateBuilder(args);

// Map settings
builder.Services.Configure<MapSettings>(builder.Configuration.GetSection("MapSettings"));

// PostgreSQL + EF
builder.Services.AddDbContext<HuntMapContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Identity (no UI): use IdentityCore instead of AddDefaultIdentity
builder.Services.AddIdentityCore<ApplicationUser>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<HuntMapContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// Cookies for auth
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme);
builder.Services.AddAuthorization();

builder.Services.AddHttpClient();      // <-- needed for IHttpClientFactory
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

// Apply schema (EnsureCreated for first run)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HuntMapContext>();
    db.Database.EnsureCreated();
    // Note: switch to db.Database.Migrate() once you add Migrations via CLI
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapHub<PinHub>("/pinHub");
app.MapFallbackToPage("/_Host");

app.Run();

public sealed class MapSettings
{
    public string ImagePath { get; set; } = "/images/map/worldmap.png";
    public int Width { get; set; } = 6000;
    public int Height { get; set; } = 5800;
}