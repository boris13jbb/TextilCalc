namespace TextilCalc.App.Services;

public sealed class UserDialogService : IUserDialogService
{
    public Task ShowAlertAsync(string title, string message, string cancel = "Aceptar") =>
        Shell.Current.DisplayAlertAsync(title, message, cancel);

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
}
