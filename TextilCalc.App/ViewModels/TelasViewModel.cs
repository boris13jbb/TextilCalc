using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextilCalc.App.Models;
using TextilCalc.App.Services;
using TextilCalc.Core.Services;

namespace TextilCalc.App.ViewModels;

public partial class TelasViewModel : ObservableObject
{
    private readonly ITelaRepository _repository;
    private readonly IExcelService _excelService;
    private readonly IFileSaver _fileSaver;
    private readonly IUserDialogService _dialogs;
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private CancellationTokenSource? _searchDebounceCancellation;

    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;
    [ObservableProperty] public partial Tela? SelectedTela { get; set; }
    [ObservableProperty] public partial string PesoBalanza { get; set; } = string.Empty;
    [ObservableProperty] public partial double? MetrajeCalculado { get; set; }
    [ObservableProperty] public partial bool IsBusy { get; set; }
    [ObservableProperty] public partial string? Error { get; set; }
    [ObservableProperty] public partial string? SuccessMessage { get; set; }
    [ObservableProperty] public partial bool IsFormVisible { get; set; }
    [ObservableProperty] public partial string FormTitle { get; set; } = "Agregar tela";
    [ObservableProperty] public partial string FormNombre { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormGramatura { get; set; } = string.Empty;
    [ObservableProperty] public partial string FormAncho { get; set; } = string.Empty;

    private int _editingId;

    public TelasViewModel(
        ITelaRepository repository,
        IExcelService excelService,
        IFileSaver fileSaver,
        IUserDialogService dialogs)
    {
        _repository = repository;
        _excelService = excelService;
        _fileSaver = fileSaver;
        _dialogs = dialogs;
    }

    public ObservableCollection<Tela> Telas { get; } = [];
    public bool HasError => !string.IsNullOrWhiteSpace(Error);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);
    public bool HasSelection => SelectedTela is not null;
    public bool HasNoSelection => SelectedTela is null;
    public bool IsNotBusy => !IsBusy;
    public bool HasCalculatedMetraje => MetrajeCalculado.HasValue;
    public string MetrajeDisplay => MetrajeCalculado?.ToString("N2") ?? "--";

    partial void OnSearchQueryChanged(string value)
    {
        _searchDebounceCancellation?.Cancel();
        _searchDebounceCancellation?.Dispose();
        _searchDebounceCancellation = new CancellationTokenSource();
        _ = DebounceSearchAsync(_searchDebounceCancellation.Token);
    }

    partial void OnSelectedTelaChanged(Tela? value)
    {
        MetrajeCalculado = null;
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(HasNoSelection));
        OnPropertyChanged(nameof(HasCalculatedMetraje));
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsNotBusy));
    partial void OnErrorChanged(string? value) => OnPropertyChanged(nameof(HasError));
    partial void OnSuccessMessageChanged(string? value) => OnPropertyChanged(nameof(HasSuccess));

    [RelayCommand]
    public async Task LoadAsync()
    {
        await _loadGate.WaitAsync();
        IsBusy = true;
        Error = null;
        try
        {
            var telas = await _repository.GetAllAsync(SearchQuery);
            Telas.Clear();
            foreach (var tela in telas)
            {
                Telas.Add(tela);
            }
        }
        catch (Exception exception)
        {
            Error = $"Error al cargar telas: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
            _loadGate.Release();
        }
    }

    private async Task DebounceSearchAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(300, cancellationToken);
            SuccessMessage = null;
            await LoadAsync();
        }
        catch (OperationCanceledException)
        {
            // A newer search value superseded this request.
        }
    }

    [RelayCommand]
    private void ShowAddForm()
    {
        _editingId = 0;
        FormTitle = "Agregar tela";
        FormNombre = string.Empty;
        FormGramatura = string.Empty;
        FormAncho = string.Empty;
        Error = null;
        SuccessMessage = null;
        IsFormVisible = true;
    }

    [RelayCommand]
    private void ShowEditForm(Tela tela)
    {
        _editingId = tela.Id;
        FormTitle = "Editar tela";
        FormNombre = tela.Nombre;
        FormGramatura = tela.Gramatura.ToString("0.##");
        FormAncho = tela.Ancho?.ToString("0.##") ?? string.Empty;
        Error = null;
        SuccessMessage = null;
        IsFormVisible = true;
    }

    [RelayCommand]
    private void CancelForm()
    {
        IsFormVisible = false;
        Error = null;
    }

    [RelayCommand]
    private async Task SaveTelaAsync()
    {
        if (string.IsNullOrWhiteSpace(FormNombre))
        {
            Error = "El nombre es obligatorio.";
            return;
        }

        if (!LocalizedNumberParser.TryParse(FormGramatura, out var gramatura) || gramatura <= 0)
        {
            Error = "Ingrese una gramatura válida (> 0).";
            return;
        }

        double? ancho = null;
        if (!string.IsNullOrWhiteSpace(FormAncho))
        {
            if (!LocalizedNumberParser.TryParse(FormAncho, out var parsedWidth) || parsedWidth <= 0)
            {
                Error = "Ingrese un ancho válido (> 0) o deje el campo vacío.";
                return;
            }

            ancho = parsedWidth;
        }

        try
        {
            await _repository.SaveAsync(new Tela
            {
                Id = _editingId,
                Nombre = FormNombre,
                Gramatura = gramatura,
                Ancho = ancho
            });
            IsFormVisible = false;
            SuccessMessage = _editingId == 0 ? "Tela agregada correctamente." : "Tela actualizada correctamente.";
            await LoadAsync();
        }
        catch (Exception exception)
        {
            Error = $"No se pudo guardar la tela: {exception.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteTelaAsync(Tela tela)
    {
        var confirmed = await _dialogs.ConfirmAsync(
            "Eliminar tela",
            $"¿Desea eliminar “{tela.Nombre}”?",
            "Eliminar",
            "Cancelar");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await _repository.DeleteAsync(tela);
            if (SelectedTela?.Id == tela.Id)
            {
                SelectedTela = null;
            }
            SuccessMessage = $"“{tela.Nombre}” se eliminó correctamente.";
            await LoadAsync();
        }
        catch (Exception exception)
        {
            Error = $"No se pudo eliminar la tela: {exception.Message}";
        }
    }

    [RelayCommand]
    private void CalculateMetraje()
    {
        SuccessMessage = null;
        if (SelectedTela is null)
        {
            Error = "Seleccione una tela.";
            return;
        }

        if (!LocalizedNumberParser.TryParse(PesoBalanza, out var peso) || peso <= 0)
        {
            Error = "Ingrese un peso válido (> 0).";
            return;
        }

        if (SelectedTela.Gramatura <= 0)
        {
            Error = "La tela debe tener una gramatura válida.";
            return;
        }

        if (SelectedTela.Ancho is null or <= 0)
        {
            Error = "La tela debe tener un ancho válido.";
            return;
        }

        MetrajeCalculado = TextileCalculator
            .CalculateMetraje(peso, SelectedTela.Gramatura, SelectedTela.Ancho.Value)
            .Metraje;
        Error = null;
        OnPropertyChanged(nameof(HasCalculatedMetraje));
        OnPropertyChanged(nameof(MetrajeDisplay));
    }

    [RelayCommand]
    private async Task ExportAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        Error = null;
        try
        {
            var telas = await _repository.GetAllAsync();
            await using var stream = await _excelService.ExportAsync(telas, cancellationToken);
            var fileName = $"telas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var result = await _fileSaver.SaveAsync(fileName, stream, cancellationToken);
            result.EnsureSuccess();
            SuccessMessage = $"Archivo guardado: {result.FilePath}";
        }
        catch (OperationCanceledException)
        {
            SuccessMessage = "Exportación cancelada.";
        }
        catch (Exception exception)
        {
            Error = $"Error al exportar: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ImportAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        Error = null;
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Seleccione el archivo Excel",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.Android] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
                    [DevicePlatform.WinUI] = [".xlsx"]
                })
            });
            if (file is null)
            {
                return;
            }

            await using var stream = await file.OpenReadAsync();
            var telas = await _excelService.ImportAsync(stream, cancellationToken);
            var result = await _repository.UpsertAsync(telas);
            SuccessMessage = $"Importación exitosa: {result.Insertadas} nuevas, {result.Actualizadas} actualizadas.";
            await LoadAsync();
        }
        catch (OperationCanceledException)
        {
            SuccessMessage = "Importación cancelada.";
        }
        catch (Exception exception)
        {
            Error = $"Error al importar: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static Task GoBackAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private static Task OpenCalculatorAsync() => Shell.Current.GoToAsync(nameof(Views.CalculadoraPage));
}
