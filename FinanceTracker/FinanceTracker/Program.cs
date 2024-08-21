using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using FinanceTracker.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FinanceDbContextConnection") ?? throw new InvalidOperationException("Connection string 'FinanceDbContextConnection' not found.");

builder.Services.AddDbContext<FinanceDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<FinanceDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

//Register Syncfusion license
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NCaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXdccHRURmVZUkdxX0o=");

builder.Services.ConfigureApplicationCookie(options =>
{
	// Redirect to login page if not authenticated
	options.LoginPath = "/Identity/Account/Login";
	// Redirect to dashboard page upon successful login
	options.Events.OnRedirectToLogin = context =>
	{
		if (context.Request.Path.Value != "/Identity/Account/Login")
		{
			context.Response.Redirect("/Identity/Account/Login");
		}
		return Task.CompletedTask;
	};
});



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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
