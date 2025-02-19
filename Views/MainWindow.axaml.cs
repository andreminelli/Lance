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

    private void UrlTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) 
            return;
        
        if (MakeRequestButton.Command?.CanExecute(null) == true)
            MakeRequestButton.Command.Execute(null);
    }
}

public record Login(string Username, string Password);