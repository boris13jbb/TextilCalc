using TextilCalc.App.ViewModels;

namespace TextilCalc.App.Views;

public partial class TelasPage : ContentPage
{
    private readonly TelasViewModel _viewModel;

    public TelasPage(TelasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
