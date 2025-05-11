using BlazorLeafletInterop.Services;
using IncludingTip.Components;
using IncludingTip.Model;
using IncludingTip.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContextFactory<TipApplicationContext>(options =>
    options
        .UseNpgsql(TipApplicationContext.ConfigureConnectionFromEnv())
        .UseSnakeCaseNamingConvention()
);

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMapService();


builder.Services.AddSingleton<RedisService>();
builder.Services.AddSingleton<CachingService>();
// builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<GenAIService>();
builder.Services.AddSingleton<NominatimService>();
builder.Services.AddSingleton<CountriesService>();
builder.Services.AddSingleton<UserService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.ExpireTimeSpan = TimeSpan.FromDays(14);
        opt.SlidingExpiration = false;
        opt.SessionStore = new SessionService(new RedisService());
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.UseAuthentication();
app.UseCookiePolicy(new CookiePolicyOptions{
    MinimumSameSitePolicy = SameSiteMode.Strict
    
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(IncludingTip.Client._Imports).Assembly);

app.Run();