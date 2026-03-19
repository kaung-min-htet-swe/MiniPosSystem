using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MiniPos.Frontend;
using MiniPos.Frontend.Mocks;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IMockMerchant, MockMerchant>();
builder.Services.AddScoped<IMockBranch, MockBranch>();
builder.Services.AddScoped<IMockCashier, MockCashier>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();