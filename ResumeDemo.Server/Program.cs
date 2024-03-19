using Microsoft.EntityFrameworkCore;
using ResumeDemo.Extensions;
using ResumeDemo.Server.Areas.Api.Models;
using ResumeDemo.Server.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(typeof(ResumeMapper).Assembly);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddResumeManager<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Resumes}/{action=Index}/{id?}");

await using (var scope = app.Services.CreateAsyncScope())
await using (var context = scope.ServiceProvider.GetRequiredService<AppDbContext>())
{
    context.Database.EnsureCreated();
}

app.Run();