using System.Net.Http.Json;
using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class CreateCategory
{
    private MudForm _form = null!;
    private CategoryCreateRequestDto _categoryModel = new();
    private List<MerchantLookupDto> _availableMerchants = new();

    private bool _isLoadingMerchants = true;
    private bool _isSaving = false;

    public record MerchantLookupDto(Guid Id, string Name);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<List<MerchantLookupDto>>("api/merchants/lookup");

            if (response != null)
            {
                _availableMerchants = response;
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to load merchants.", Severity.Error);
        }
        finally
        {
            _isLoadingMerchants = false;
        }
    }

    private async Task SubmitForm()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _isSaving = true;

        try
        {
            // Send the data to your ASP.NET Core API
            var response = await Http.PostAsJsonAsync("api/categories", _categoryModel);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Category created successfully!", Severity.Success);
                Navigation.NavigateTo("/categories");
            }
            else
            {
                Snackbar.Add("Failed to create category. Please verify your inputs.", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("A network error occurred while communicating with the server.", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/categories");
    }
}