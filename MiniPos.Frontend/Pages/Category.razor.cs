using System.Net.Http.Json;
using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class Category
{
    private string _searchString = string.Empty;
    private MudTable<CategoryResponseDto> _table = null!;

    private async Task<TableData<CategoryResponseDto>> ServerReload(TableState state, CancellationToken token)
    {
        var pageNumber = state.Page + 1;
        var pageSize = state.PageSize;

        var url = $"api/categories?pageNumber={pageNumber}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(_searchString)) url += $"&searchTerm={Uri.EscapeDataString(_searchString)}";

        if (!string.IsNullOrEmpty(state.SortLabel))
            url += $"&sortBy={state.SortLabel}&isDescending={state.SortDirection == SortDirection.Descending}";

        try
        {
            var response = await Http.GetFromJsonAsync<PagedResult<CategoryResponseDto>>(url);

            if (response != null)
                return new TableData<CategoryResponseDto>
                {
                    TotalItems = response.TotalCount,
                    Items = response.Data
                };
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to fetch categories. Please check your connection.", Severity.Error);
        }

        return new TableData<CategoryResponseDto> { TotalItems = 0, Items = new List<CategoryResponseDto>() };
    }

    private void OnSearch(string text)
    {
        _searchString = text;
        _table.ReloadServerData();
    }

    private async Task PromptDelete(string id, string categoryName)
    {
        // We will implement the MudDialog confirmation here shortly!
        Snackbar.Add($"Delete clicked for {categoryName}", Severity.Info);
    }
}