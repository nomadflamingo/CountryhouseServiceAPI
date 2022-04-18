using CloudinaryDotNet;
using CountryhouseService.API.Data;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using CountryhouseService.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure services
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<Cloudinary>((sp) =>
{
    IConfigurationSection cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings");
    Account account = new(
        cloudinarySettings["Name"],
        cloudinarySettings["ApiKey"],
        cloudinarySettings["ApiSecret"]);
    return new Cloudinary(account);
});

// Register db context
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(connection,
        sqlServerOptionsAction: options =>
        {
            options.EnableRetryOnFailure();
        }));

// Register unit of work and repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAdsRepository, AdsRepository>();
builder.Services.AddScoped<IRequestsRepository, RequestsRepository>();
builder.Services.AddScoped<IImagesRepository<AdImage>, ImagesRepository<AdImage>>();
builder.Services.AddScoped<IImagesRepository<Avatar>, ImagesRepository<Avatar>>();
builder.Services.AddScoped<IStatusesRepository<AdStatus>, StatusesRepository<AdStatus>>();
builder.Services.AddScoped<IStatusesRepository<RequestStatus>, StatusesRepository<RequestStatus>>();

// Add identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Configure default redirect actions
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});


// Configure middleware
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

// Db seeding
var scopedFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopedFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await Initializer.InvokeAsync(roleManager, db);
}

app.Run();
