using TextilCalc.App.ViewModels;

namespace TextilCalc.App.Views;

public partial class MetrajePage : ContentPage
{
    public MetrajePage(MetrajeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
