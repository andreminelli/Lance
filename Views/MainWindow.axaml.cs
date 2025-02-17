using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace Lance.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TextBox_OnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        IClipboard? clipboard = GetTopLevel(this)?.Clipboard;
        
        if (clipboard is null)
            return;

        Task<string?> data = clipboard.GetTextAsync();
        
        while (!data.IsCompleted)
        {
            if (data.IsCanceled || data.IsFaulted)
                return;
        }

        if (sender is not TextBox textBox)
            return;

        if (string.IsNullOrEmpty(data.Result) || string.IsNullOrWhiteSpace(data.Result))
            return;
        
        textBox.Text = data.Result;
    }

    private void UrlTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) 
            return;
        
        if (MakeRequestButton.Command?.CanExecute(null) == true)
            MakeRequestButton.Command.Execute(null);
    }
}

public record Login(string Username, string Password);