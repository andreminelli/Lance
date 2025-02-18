using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lance.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(MakeRequestCommand))]
    private string _url = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MakeRequestCommand))]
    private HttpMethod? _selectedMethod = HttpMethod.Get;

    [ObservableProperty] private string? _body;

    [ObservableProperty] private bool _requestCompleted;

    [ObservableProperty] private string? _formattedRequestTime;

    [ObservableProperty] private string? _formattedStatusCode;

    [ObservableProperty] private string? _responseContent;
    
    public HttpMethod[] HttpMethods { get; } =
        [HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete, HttpMethod.Options];

    [RelayCommand(CanExecute = nameof(CanMakeRequest))]
    private async Task MakeRequest()
    {
        using HttpClient client = new();

        Stopwatch stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response =
            await client.SendAsync(new HttpRequestMessage(SelectedMethod, Url));
        stopwatch.Stop();

        await UpdateResponseData(response, stopwatch.Elapsed);
    }

    [MemberNotNullWhen(true, nameof(SelectedMethod))]
    private bool CanMakeRequest() =>
        Uri.TryCreate(Url, UriKind.Absolute, out var _) && SelectedMethod is not null;

    private async Task UpdateResponseData(HttpResponseMessage response, TimeSpan requestDuration)
    {
        Task<string> stringResponseContent = response.Content.ReadAsStringAsync();
        
        FormattedRequestTime = $"{Math.Round(requestDuration.TotalMilliseconds)} ms";

        FormattedStatusCode = $"{(int)response.StatusCode} - {response.ReasonPhrase}";

        await SetResponseContent(response, stringResponseContent);

        RequestCompleted = true;
    }

    private async Task SetResponseContent(HttpResponseMessage response, Task<string> stringResponseContent)
    {
        if (response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string>? contentTypes))
        {
            if (contentTypes.Any(a => a.Contains("application/json")))
            {
                object? obj = JsonSerializer.Deserialize<object?>(await stringResponseContent);

                if (obj is not null)
                {
                    ResponseContent = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                    return;
                }
            }
        }
        
        ResponseContent = await stringResponseContent;
    }
}