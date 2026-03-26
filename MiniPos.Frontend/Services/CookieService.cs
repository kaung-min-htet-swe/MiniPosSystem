using Microsoft.JSInterop;

namespace MiniPos.Frontend.Services;

public class CookieService
{
    private readonly IJSRuntime _jsRuntime;

    public CookieService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetCookie(string name, string value, int? days = null)
    {
        await _jsRuntime.InvokeVoidAsync("cookieHelper.setCookie", name, value, days);
    }

    public async Task<string?> GetCookie(string name)
    {
        return await _jsRuntime.InvokeAsync<string?>("cookieHelper.getCookie", name);
    }

    public async Task DeleteCookie(string name)
    {
        await _jsRuntime.InvokeVoidAsync("cookieHelper.deleteCookie", name);
    }

    public async Task<Guid?> GetUserId()
    {
        var idStr = await GetCookie("X-User-Id");
        if (Guid.TryParse(idStr, out var id))
        {
            return id;
        }
        return null;
    }
}
