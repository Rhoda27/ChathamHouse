using ChathamHouse.Data;
using ChathamHouse.Models;
using ChathamHouse.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Single registrations - no duplicates
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR();
//builder.Services.AddHttpClient();
builder.Services.AddLogging();

// ✅ Single DbContext registration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.";
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Form/Login";
    options.AccessDeniedPath = "/Form/Login";
});

builder.Services.AddHttpClient<IPaystackService, PaystackService>(client =>
{
    client.BaseAddress = new Uri("https://api.paystack.co");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["Paystack:SecretKey"]}");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12
    };

    // For development only - remove this in production
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});
//builder.Services.AddScoped<IPaystackService, PaystackService>();
builder.Services.AddScoped<IRatingService, RatingService>();

var app = builder.Build();

// ✅ Correct middleware order
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["ngrok-skip-browser-warning"] = "true";
    await next();
});


app.UseStaticFiles();// ✅ moved UP - before UseRouting
app.UseRouting();
app.UseAuthentication();    // ✅ must be before UseAuthorization
app.UseAuthorization();

// Role seeding
using var scope = app.Services.CreateScope();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
string[] roles = new[] { "Admin", "User" };
foreach (var role in roles)
{
    if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new IdentityRole(role));
}

// TEMPORARY DEBUG - add this before app.MapRazorPages()
app.MapGet("/debug-routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var endpoints = endpointSources
        .SelectMany(es => es.Endpoints)
        .Select(e => e.DisplayName)
        .ToList();
    return endpoints;
});



app.MapRazorPages();
app.MapControllers();

app.Run();