using System.Net.Http.Json;
using MiniPos.Frontend.Components;
using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class Cashier
{
    private MudTable<CashierResponseDto> _table = null!;
    private string _searchString = string.Empty;

    private async Task<TableData<CashierResponseDto>> CashierReload(TableState state, CancellationToken token)
    {
        var pageNumber = state.Page + 1;
        var pageSize = state.PageSize;
        var url = $"api/users/cashiers?pageNumber={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(_searchString))
        {
            url += $"&searchTerm={Uri.EscapeDataString(_searchString)}";
        }

        if (!string.IsNullOrEmpty(state.SortLabel))
        {
            url += $"&sortBy={state.SortLabel}&isDescending={state.SortDirection == SortDirection.Descending}";
        }

        try
        {
            var response = await Http.GetFromJsonAsync<PagedResult<CashierResponseDto>>(url, token);
            if (response != null)
            {
                return new TableData<CashierResponseDto>
                {
                    TotalItems = response.TotalCount,
                    Items = response.Data,
                };
            }
        }
        catch (Exception e)
        {
        }

        return new TableData<CashierResponseDto> { TotalItems = 0, Items = new List<CashierResponseDto>() };
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _table.ReloadServerData();
    }
    
    private async Task PromptDeleteCashier(string id, string username)
    {
        var parameters = new DialogParameters<ConfirmDeleteDialog>
        {
            { x => x.Title, "Delete Cashier" },
            { x => x.ContentText, $"Are you sure you want to delete the cashier account for '{username}'?" }
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        var dialog = await DialogService.ShowAsync<ConfirmDeleteDialog>("Delete", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            try
            {
                var response = await Http.DeleteAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add($"Cashier '{username}' was successfully deleted.", Severity.Success);
                
                    await _table.ReloadServerData();
                }
                else
                {
                    // Optionally extract your ProblemDetails here
                    Snackbar.Add("Failed to delete cashier. They might have processed orders that prevent deletion.", Severity.Error);
                }
            }
            catch (Exception)
            {
                Snackbar.Add("A network error occurred.", Severity.Error);
            }
        }
    }
}