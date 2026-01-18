using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using BrowserFileManger.Data;
using BrowserFileManger.Services;

var builder = WebApplication.CreateBuilder(args);
var migrateOnly = args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

// Configure SQLite Database
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "app.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register services
builder.Services.AddSingleton<FileService>();
builder.Services.AddScoped<TrackService>();
builder.Services.AddScoped<AlbumService>();
builder.Services.AddScoped<ArtistService>();
builder.Services.AddScoped<MetadataSyncService>();

var app = builder.Build();

// Apply migrations and import existing files on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    if (!migrateOnly)
    {
        // Import any existing files not in database
        var syncService = scope.ServiceProvider.GetRequiredService<MetadataSyncService>();
        var (imported, removed) = await syncService.FullSyncAsync();
        if (imported > 0 || removed > 0)
        {
            Console.WriteLine($"Metadata sync: {imported} files imported, {removed} orphaned records removed");
        }
    }
}

if (migrateOnly)
{
    return;
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