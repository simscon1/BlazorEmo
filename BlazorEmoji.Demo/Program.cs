using BlazorEmoji.Demo;
using BlazorEmoji.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register BlazorEmoji services - pass the base address
builder.Services.AddEmojiServices(builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();
