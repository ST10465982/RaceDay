using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

var builder = WebApplication.CreateBuilder(args);


// =============================================================
// DATABASE
// =============================================================

builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RaceDayConnection")
    ));


// =============================================================
// CONTROLLERS
// =============================================================

builder.Services.AddControllers();


// =============================================================
// SESSION AUTHENTICATION
// =============================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;

    options.Cookie.SameSite = SameSiteMode.Lax;
});


// =============================================================
// PASSWORD HASHING
// =============================================================

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


// =============================================================
// SWAGGER / OPENAPI
// =============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


var app = builder.Build();


// =============================================================
// HTTP PIPELINE
// =============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// Session must be enabled before controller endpoints run.
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();