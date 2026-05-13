using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Modelbouwer.Mobile.Models;
using Modelbouwer.Mobile.Services;

namespace Modelbouwer.Mobile.ViewModels;

public partial class ProductsViewModel : BaseViewModel
{
    private readonly IMobileWorkspaceService workspace;

    [ObservableProperty] private MobileProduct? selectedProduct;
    [ObservableProperty] private MobileCategory? selectedCategory;
    [ObservableProperty] private MobileUnit? selectedUnit;

    public ProductsViewModel(IMobileWorkspaceService workspace)
    {
        this.workspace = workspace;
        Title = "Producten";
    }

    public ObservableCollection<MobileProduct> Products => workspace.Products;
    public ObservableCollection<MobileCategory> Categories => workspace.Categories;
    public ObservableCollection<MobileUnit> Units => workspace.Units;

    [RelayCommand]
    private Task LoadAsync()
    {
        return RunBusyAsync(async () =>
        {
            await workspace.LoadAsync();
            SelectedProduct ??= Products.FirstOrDefault();
            SyncSelectionsFromProduct();
        }, "Database geladen.");
    }

    [RelayCommand]
    private async Task NewProductAsync()
    {
        var category = SelectedCategory ?? Categories.FirstOrDefault();
        var unit = SelectedUnit ?? Units.FirstOrDefault();
        var product = new MobileProduct
        {
            Code = "NIEUW",
            Name = "Nieuw product",
            CategoryId = category?.Id ?? 0,
            Category = category?.Name ?? string.Empty,
            UnitId = unit?.Id ?? 0,
            Unit = unit?.Name ?? string.Empty
        };
        await RunBusyAsync(async () =>
        {
            await workspace.AddProductAsync(product);
            SelectedProduct = product;
        }, "Nieuw product toegevoegd.");
    }

    [RelayCommand]
    private Task SaveProductAsync()
    {
        return RunBusyAsync(async () =>
        {
            if (SelectedProduct is null)
            {
                StatusText = "Kies een product.";
                return;
            }

            await workspace.UpdateProductAsync(SelectedProduct);
        }, "Productgegevens bijgewerkt.");
    }

    partial void OnSelectedProductChanged(MobileProduct? value)
    {
        SyncSelectionsFromProduct();
    }

    partial void OnSelectedCategoryChanged(MobileCategory? value)
    {
        if (SelectedProduct is null || value is null)
            return;

        SelectedProduct.CategoryId = value.Id;
        SelectedProduct.Category = value.Name;
    }

    partial void OnSelectedUnitChanged(MobileUnit? value)
    {
        if (SelectedProduct is null || value is null)
            return;

        SelectedProduct.UnitId = value.Id;
        SelectedProduct.Unit = value.Name;
    }

    private void SyncSelectionsFromProduct()
    {
        if (SelectedProduct is null)
            return;

        SelectedCategory = Categories.FirstOrDefault(category => category.Id == SelectedProduct.CategoryId);
        SelectedUnit = Units.FirstOrDefault(unit => unit.Id == SelectedProduct.UnitId);
    }
}
