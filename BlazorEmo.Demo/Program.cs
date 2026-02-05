using BlazorEmo.Demo;
using BlazorEmo.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

 
// Register BlazorEmo services
builder.Services.AddEmoServices();

await builder.Build().RunAsync();
