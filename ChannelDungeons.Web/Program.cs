using ChannelDungeons.Web;
using ChannelDungeons.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<IChannelCatalog, ChannelCatalog>();
builder.Services.AddSingleton<IDelayProvider, TaskDelayProvider>();
builder.Services.AddSingleton(TimeProvider.System);

await builder.Build().RunAsync();
