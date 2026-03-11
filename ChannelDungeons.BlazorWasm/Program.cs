using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ChannelDungeons.BlazorWasm;
using ChannelDungeons.BlazorWasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for loading configuration
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register application services
builder.Services.AddScoped<ChannelService>();
builder.Services.AddScoped<MessageAnimationService>();
builder.Services.AddScoped<SidebarAnimationService>();

await builder.Build().RunAsync();
