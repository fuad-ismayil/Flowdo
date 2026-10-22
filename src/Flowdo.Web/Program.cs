using Dapper;
using Npgsql;
using Flowdo.Web.Data;

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
