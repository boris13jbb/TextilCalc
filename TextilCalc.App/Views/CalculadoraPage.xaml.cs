using TextilCalc.App.ViewModels;

namespace TextilCalc.App.Views;

public partial class CalculadoraPage : ContentPage
{
    public CalculadoraPage(CalculadoraViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
