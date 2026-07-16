using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ChannelDungeons.Web;
using ChannelDungeons.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<BrowserInterop>();
builder.Services.AddSingleton<IDelayProvider, DelayProvider>();

await builder.Build().RunAsync();
