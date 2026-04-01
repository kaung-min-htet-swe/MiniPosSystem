using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MiniPos.Frontend;
using MiniPos.Frontend.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<CredentialsHandler>();
builder.Services.AddHttpClient("MiniPos.Api",
        client => client.BaseAddress = new Uri("http://localhost:5107/"))
    .AddHttpMessageHandler<CredentialsHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MiniPos.Api"));
builder.Services.AddScoped<CookieService>();

await builder.Build().RunAsync();