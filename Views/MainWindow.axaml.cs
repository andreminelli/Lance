using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;

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
        IEnumerable<Login> logins = Enumerable.Range(0, 10).Select(_ => GetLoginObject());

        ResponseContentTextBox.Text =
            JsonSerializer.Serialize(logins, new JsonSerializerOptions { WriteIndented = true });
    }

    private static Login GetLoginObject() => new("login", "password");
}

public record Login(string Username, string Password);