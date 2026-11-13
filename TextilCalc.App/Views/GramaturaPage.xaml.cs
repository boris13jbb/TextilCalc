using TextilCalc.App.ViewModels;

namespace TextilCalc.App.Views;

public partial class GramaturaPage : ContentPage
{
    public GramaturaPage(GramaturaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// En Android el texto del Entry a menudo no llega al ViewModel hasta Unfocus/Completed.
    /// Se fuerza el commit antes de calcular.
    /// </summary>
    private async void OnCalculateClicked(object? sender, EventArgs e)
    {
        LengthEntry.Unfocus();
        WidthEntry.Unfocus();
        WeightEntry.Unfocus();
        await Task.Delay(40);

        if (BindingContext is GramaturaViewModel viewModel &&
            viewModel.CalculateCommand.CanExecute(null))
        {
            viewModel.CalculateCommand.Execute(null);
        }
    }
}
