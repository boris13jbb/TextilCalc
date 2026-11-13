using TextilCalc.App.Services;
using TextilCalc.App.ViewModels;
using Microsoft.Extensions.Logging;

namespace TextilCalc.App.Views;

public partial class HomePage : ContentPage
{
    private readonly NavigationTutorialService _tutorialService;
    private readonly ILogger<HomePage> _logger;

    public HomePage(
        HomeViewModel viewModel,
        NavigationTutorialService tutorialService,
        ILogger<HomePage> logger)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _tutorialService = tutorialService;
        _logger = logger;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _tutorialService.ShowIfNeededAsync();
        }
        catch (Exception exception)
        {
            // El tutorial no debe impedir el arranque ni el uso de las herramientas.
            _logger.LogError(exception, "No se pudo mostrar el tutorial de navegación.");
        }
    }
}
