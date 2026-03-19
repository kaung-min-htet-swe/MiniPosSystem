using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class CashierView
{
    private CashierGetByIdResponseDto? _cashier;
    private bool _isLoading = true;
    [Parameter] public string Id { get; set; } = string.Empty;

    private async Task GetCashier(string id)
    {
        try
        {
            _cashier = await Http.GetFromJsonAsync<CashierGetByIdResponseDto>($"api/users/{Id}");
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
                _cashier = null;
            else
                Snackbar.Add("A network error occurred while fetching user data.", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetCashierMock(Id);
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/cashiers");
    }
}