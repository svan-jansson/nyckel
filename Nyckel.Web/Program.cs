using Nyckel.Core;
using Nyckel.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddLogging();
builder.Services.AddSingleton(Config.Backend());
builder.Services.AddSingleton<INyckel>(provider =>
    new NyckelObservabilityDecorator(
        (INyckel)provider.GetRequiredService(Config.Backend()),
        provider.GetRequiredService<ILogger<INyckel>>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run($"http://*:{Config.Port()}");
