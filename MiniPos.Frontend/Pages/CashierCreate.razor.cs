using System.Net.Http.Json;
using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class CashierCreate
{
    private List<LookupItemDto> _availableBranches = new();
    private List<LookupItemDto> _availableMerchants = new();
    private readonly CashierCreateRequestDto _cashierModel = new();
    private MudForm _form = null!;
    private bool _isLoadingBranches;
    private bool _isLoadingMerchants = true;
    private bool _isSaving;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private bool _showPassword;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<List<LookupItemDto>>("api/merchants/lookup");
            if (response != null) _availableMerchants = response;
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

    private void TogglePasswordVisibility()
    {
        if (_showPassword)
        {
            _showPassword = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _showPassword = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }

    private async Task OnMerchantChanged(string newMerchantId)
    {
        _cashierModel.MerchantId = newMerchantId;
        _cashierModel.BranchId = null;
        _availableBranches.Clear();
        await LoadBranchesForMerchant(newMerchantId);
    }

    private async Task LoadBranchesForMerchant(string merchantId)
    {
        _isLoadingBranches = true;
        StateHasChanged();

        try
        {
            var response =
                await Http.GetFromJsonAsync<List<LookupItemDto>>($"api/branches/lookup?merchantId={merchantId}");
            if (response != null) _availableBranches = response;
        }
        catch
        {
            Snackbar.Add("Failed to load branches for the selected merchant.", Severity.Warning);
        }
        finally
        {
            _isLoadingBranches = false;
            StateHasChanged();
        }
    }

    private async Task SubmitForm()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _isSaving = true;

        try
        {
            // Note: Adjust the URL to match your exact POST endpoint
            var response = await Http.PostAsJsonAsync("api/users", _cashierModel);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Cashier created successfully!", Severity.Success);
                Navigation.NavigateTo("/cashiers");
            }
            else
            {
                Snackbar.Add("Failed to create cashier. Username or email might already exist.", Severity.Error);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("A network error occurred.", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/cashiers");
    }

    public record LookupItemDto(Guid Id, string Name);
}