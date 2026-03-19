using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MiniPos.Frontend.Models;
using MudBlazor;

namespace MiniPos.Frontend.Pages;

public partial class CashierEdit
{
    private List<LookupItemDto> _availableBranches = new();

    private List<LookupItemDto> _availableMerchants = new();
    private CashierUpdateRequestDto? _cashierModel;

    private MudForm _form = null!;

    private bool _isLoading = true;
    private bool _isLoadingBranches;
    private bool _isSaving;
    [Parameter] public string Id { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var existingCashier = await Http.GetFromJsonAsync<CashierGetByIdResponseDto>($"api/users/{Id}");
            var merchantsTask = Http.GetFromJsonAsync<List<LookupItemDto>>("api/merchants/lookup");
            if (existingCashier != null)
            {
                _cashierModel = new CashierUpdateRequestDto
                {
                    Email = existingCashier.Email ?? "",
                    MerchantId = existingCashier.Merchant?.Id != null ? Guid.Parse(existingCashier.Merchant.Id) : null,
                    BranchId = existingCashier.Branch?.Id != null ? Guid.Parse(existingCashier.Branch.Id) : null
                };

                _availableMerchants = await merchantsTask ?? new();

                if (_cashierModel.MerchantId.HasValue) await LoadBranchesForMerchant(_cashierModel.MerchantId.Value);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to load cashier data.", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task OnMerchantChanged(Guid? newMerchantId)
    {
        _cashierModel!.MerchantId = newMerchantId;
        _cashierModel.BranchId = null;
        _availableBranches.Clear();

        if (newMerchantId.HasValue) await LoadBranchesForMerchant(newMerchantId.Value);
    }

    private async Task LoadBranchesForMerchant(Guid merchantId)
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

    private async Task SaveChanges()
    {
        await _form.ValidateAsync();
        if (!_form.IsValid) return;

        _isSaving = true;

        try
        {
            var response = await Http.PutAsJsonAsync($"api/users/{Id}", _cashierModel);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Cashier updated successfully!", Severity.Success);
                Navigation.NavigateTo("/cashiers");
            }
            else
            {
                Snackbar.Add("Failed to update cashier. Please check inputs.", Severity.Error);
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