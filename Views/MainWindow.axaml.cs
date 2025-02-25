using Avalonia.Controls;
using Avalonia.Input;

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