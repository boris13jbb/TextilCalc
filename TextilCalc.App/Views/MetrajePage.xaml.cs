using TextilCalc.App.ViewModels;

namespace TextilCalc.App.Views;

public partial class MetrajePage : ContentPage
{
    public MetrajePage(MetrajeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// En Android el texto del Entry a menudo no llega al ViewModel hasta Unfocus/Completed.
    /// Se fuerza el commit antes de calcular y se acerca el resultado al viewport del ScrollView.
    /// </summary>
    private async void OnCalculateClicked(object? sender, EventArgs e)
    {
        WeightEntry.Unfocus();
        GsmEntry.Unfocus();
        WidthEntry.Unfocus();
        await Task.Delay(40);

        if (BindingContext is MetrajeViewModel viewModel &&
            viewModel.CalculateCommand.CanExecute(null))
        {
            viewModel.CalculateCommand.Execute(null);
        }

        if (ResultCard.IsVisible)
        {
            await ContentScroll.ScrollToAsync(ResultCard, ScrollToPosition.MakeVisible, true);
        }
    }
}
