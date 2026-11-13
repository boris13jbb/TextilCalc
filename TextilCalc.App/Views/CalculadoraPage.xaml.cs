using TextilCalc.App.ViewModels;

namespace TextilCalc.App.Views;

public partial class CalculadoraPage : ContentPage
{
    private readonly CalculadoraViewModel _viewModel;

#if WINDOWS
    private Microsoft.UI.Xaml.UIElement? _platformView;
#endif

    public CalculadoraPage(CalculadoraViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AttachKeyboardHandlers();
    }

    protected override void OnDisappearing()
    {
        DetachKeyboardHandlers();
        base.OnDisappearing();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        AttachKeyboardHandlers();
    }

    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.IsHistoryVisible)
        {
            _viewModel.IsHistoryVisible = false;
            return true;
        }

        return base.OnBackButtonPressed();
    }

    private void AttachKeyboardHandlers()
    {
#if WINDOWS
        DetachKeyboardHandlers();
        if (Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            _platformView = element;
            element.KeyDown += OnWindowsKeyDown;
            element.CharacterReceived += OnWindowsCharacterReceived;
            if (element is Microsoft.UI.Xaml.Controls.Control control)
            {
                control.IsTabStop = true;
                control.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            }
        }
#endif
    }

    private void DetachKeyboardHandlers()
    {
#if WINDOWS
        if (_platformView is not null)
        {
            _platformView.KeyDown -= OnWindowsKeyDown;
            _platformView.CharacterReceived -= OnWindowsCharacterReceived;
            _platformView = null;
        }
#endif
    }

#if WINDOWS
    // KeyDown solo para teclas de control / teclado numérico que no generan CharacterReceived útil.
    // CharacterReceived cubre dígitos y operadores tipográficos para evitar doble ingreso.
    private void OnWindowsKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        var mapped = e.Key switch
        {
            Windows.System.VirtualKey.Enter or Windows.System.VirtualKey.GamepadA => "=",
            Windows.System.VirtualKey.Back => "Backspace",
            Windows.System.VirtualKey.Escape => "Escape",
            Windows.System.VirtualKey.NumberPad0 => "0",
            Windows.System.VirtualKey.NumberPad1 => "1",
            Windows.System.VirtualKey.NumberPad2 => "2",
            Windows.System.VirtualKey.NumberPad3 => "3",
            Windows.System.VirtualKey.NumberPad4 => "4",
            Windows.System.VirtualKey.NumberPad5 => "5",
            Windows.System.VirtualKey.NumberPad6 => "6",
            Windows.System.VirtualKey.NumberPad7 => "7",
            Windows.System.VirtualKey.NumberPad8 => "8",
            Windows.System.VirtualKey.NumberPad9 => "9",
            Windows.System.VirtualKey.Add => "+",
            Windows.System.VirtualKey.Subtract => "-",
            Windows.System.VirtualKey.Multiply => "×",
            Windows.System.VirtualKey.Divide => "÷",
            Windows.System.VirtualKey.Decimal => ".",
            _ => null
        };

        if (mapped is null)
        {
            return;
        }

        _viewModel.HandleKeyCommand.Execute(mapped);
        e.Handled = true;
    }

    private void OnWindowsCharacterReceived(object sender, Microsoft.UI.Xaml.Input.CharacterReceivedRoutedEventArgs e)
    {
        var mapped = e.Character switch
        {
            '%' => "%",
            '(' => "(",
            ')' => ")",
            '^' => "^",
            '*' => "×",
            '/' => "÷",
            '+' => "+",
            '-' => "-",
            '=' => "=",
            '.' or ',' => ".",
            >= '0' and <= '9' => e.Character.ToString(),
            _ => null
        };

        if (mapped is null)
        {
            return;
        }

        _viewModel.HandleKeyCommand.Execute(mapped);
        e.Handled = true;
    }
#endif

#if ANDROID
    /// <summary>
    /// Recibe teclas físicas desde MainActivity.
    /// </summary>
    public bool TryHandleAndroidKey(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
    {
        if (e.Action != Android.Views.KeyEventActions.Down)
        {
            return false;
        }

        var mapped = MapAndroidKey(keyCode, e);
        if (mapped is null)
        {
            return false;
        }

        _viewModel.HandleKeyCommand.Execute(mapped);
        return true;
    }

    private static string? MapAndroidKey(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
    {
        if (keyCode is >= Android.Views.Keycode.Num0 and <= Android.Views.Keycode.Num9)
        {
            return ((int)(keyCode - Android.Views.Keycode.Num0)).ToString();
        }

        if (keyCode is >= Android.Views.Keycode.Numpad0 and <= Android.Views.Keycode.Numpad9)
        {
            return ((int)(keyCode - Android.Views.Keycode.Numpad0)).ToString();
        }

        var fromKey = keyCode switch
        {
            Android.Views.Keycode.Plus or Android.Views.Keycode.NumpadAdd => "+",
            Android.Views.Keycode.Minus or Android.Views.Keycode.NumpadSubtract => "-",
            Android.Views.Keycode.NumpadMultiply => "×",
            Android.Views.Keycode.Slash or Android.Views.Keycode.NumpadDivide => "÷",
            Android.Views.Keycode.Period or Android.Views.Keycode.Comma or Android.Views.Keycode.NumpadDot => ".",
            Android.Views.Keycode.Enter or Android.Views.Keycode.NumpadEnter or Android.Views.Keycode.Equals => "=",
            Android.Views.Keycode.Del => "Backspace",
            Android.Views.Keycode.Escape => "Escape",
            _ => null
        };
        if (fromKey is not null)
        {
            return fromKey;
        }

        return e.UnicodeChar switch
        {
            '%' => "%",
            '^' => "^",
            '(' => "(",
            ')' => ")",
            '*' => "×",
            '/' => "÷",
            '+' => "+",
            '-' => "-",
            '.' or ',' => ".",
            >= '0' and <= '9' => ((char)e.UnicodeChar).ToString(),
            _ => null
        };
    }
#endif
}
