using IncludingTip.Components;
using IncludingTip.Model;
using IncludingTip.Services;
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

builder.Services.AddSingleton<GenAIService>();
builder.Services.AddSingleton<NominatimService>();
builder.Services.AddSingleton<CountriesService>();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(IncludingTip.Client._Imports).Assembly);

app.Run();