namespace TextilCalc.App.Services;

public interface IUserDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel = "Aceptar");
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}
