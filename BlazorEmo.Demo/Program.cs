using BlazorEmo.Demo;
using BlazorEmo.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register BlazorEmo services - pass the base address
builder.Services.AddEmoServices(builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();
