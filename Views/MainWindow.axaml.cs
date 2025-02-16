using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace Lance.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        FillResponseTextBoxWithJsonArray();
    }

    private void FillResponseTextBoxWithJsonArray()
    {
        IEnumerable<Login> logins = Enumerable.Range(0, 30).Select(_ => GetLoginObject());

        ResponseContentTextBox.Text =
            JsonSerializer.Serialize(logins, new JsonSerializerOptions { WriteIndented = true });
    }

    private static Login GetLoginObject() => new("login", "password");

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
}

public record Login(string Username, string Password);