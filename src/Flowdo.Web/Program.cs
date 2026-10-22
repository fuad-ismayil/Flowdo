using System.Security.Claims;using Dapper;
using Npgsql;
using Flowdo.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    var connectionString = builder.Configuration
        .GetConnectionString("Default")
        ?? throw new InvalidOperationException(
            "Database connection string not found."
        );

    return NpgsqlDataSource.Create(connectionString);
});

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<TaskRepository>();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId =
            builder.Configuration[
                "Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException(
                "Google Client ID is missing.");

        options.ClientSecret =
            builder.Configuration[
                "Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException(
                "Google Client Secret is missing.");

        options.CallbackPath = "/signin-google";

        options.SaveTokens = false;

        options.Events.OnCreatingTicket = async context =>
{
    var googleSub = context.Principal?
        .FindFirstValue(ClaimTypes.NameIdentifier);

    var email = context.Principal?
        .FindFirstValue(ClaimTypes.Email);

    var displayName = context.Principal?
        .FindFirstValue(ClaimTypes.Name);

    if (string.IsNullOrWhiteSpace(googleSub) ||
        string.IsNullOrWhiteSpace(email))
    {
        throw new InvalidOperationException(
            "Required Google identity claims are missing.");
    }

    var repository = context.HttpContext
        .RequestServices
        .GetRequiredService<UserRepository>();

    var appUserId = await repository.UpsertAsync(
        googleSub,
        email,
        string.IsNullOrWhiteSpace(displayName)
            ? email
            : displayName,
        context.HttpContext.RequestAborted
    );

    context.Identity!.AddClaim(
        new Claim("app_user_id", appUserId.ToString())
    );
};
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();

    var repository = scope.ServiceProvider
        .GetRequiredService<TaskRepository>();

    var tasks = await repository.GetAllAsync(Guid.Empty);

    app.Logger.LogInformation(
        "TaskRepository working! Result count: {Count}",
        tasks.Count
    );
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
