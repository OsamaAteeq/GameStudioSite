using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GameStudioSite.Data;
using GameStudioSite.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);
var keysPath = builder.Configuration["DataProtection:KeysPath"];

if (!string.IsNullOrEmpty(keysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("GameStudioSite");
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false; // relax default complexity, your call
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/Login";
});
builder.Services.AddHttpClient("GameUrlValidation")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            ConnectTimeout = TimeSpan.FromSeconds(5),

            ConnectCallback = async (context, cancellationToken) =>
            {
                IPAddress[] addresses;

                try
                {
                    addresses = await Dns.GetHostAddressesAsync(
                        context.DnsEndPoint.Host,
                        cancellationToken);
                }
                catch (SocketException ex)
                {
                    throw new HttpRequestException(
                        "The hostname could not be resolved.",
                        ex);
                }

                if (addresses.Length == 0 ||
                    addresses.Any(a => !PublicUrlValidator.IsPublicAddress(a)))
                {
                    throw new HttpRequestException(
                        "The hostname resolves to a non-public IP address.");
                }

                Exception? lastError = null;

                foreach (var address in addresses)
                {
                    var socket = new Socket(
                        address.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp)
                    {
                        NoDelay = true
                    };

                    try
                    {
                        await socket.ConnectAsync(
                            new IPEndPoint(address, context.DnsEndPoint.Port),
                            cancellationToken);

                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch (Exception ex) when (
                        ex is SocketException or OperationCanceledException)
                    {
                        socket.Dispose();
                        lastError = ex;

                        if (ex is OperationCanceledException)
                            throw;
                    }
                }

                throw new HttpRequestException(
                    "Could not connect to the public hostname.",
                    lastError);
            }
        });

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<GameRepository>();
builder.Services.AddScoped<BlogPostRepository>();
builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddScoped<CategoryRepository>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<MongoDbContext>().EnsureIndexes(app.Logger);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var adminEmail = builder.Configuration["AdminAccount:Email"];
    var adminPassword = builder.Configuration["AdminAccount:Password"];

    if (!string.IsNullOrEmpty(adminEmail) && await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, adminPassword!);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
