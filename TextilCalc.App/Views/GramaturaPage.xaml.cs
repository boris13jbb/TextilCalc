using TextilCalc.App.ViewModels;

namespace TextilCalc.App.Views;

public partial class GramaturaPage : ContentPage
{
    public GramaturaPage(GramaturaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
