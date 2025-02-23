using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lance.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(MakeRequestCommand))]
    private string _url = string.Empty;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(MakeRequestCommand))]
    private HttpMethod _selectedMethod = HttpMethod.Get;

    [ObservableProperty] private string? _body;

    [ObservableProperty] private bool _requestCompleted;

    [ObservableProperty] private string? _formattedRequestTime;

    [ObservableProperty] private string? _formattedStatusCode;

    [ObservableProperty] private string? _responseContent;

    [ObservableProperty] private ObservableCollection<RequestHeader> _requestHeaders =
    [
        new ("Accept", "application/json"),
        new ("Content-Type", "application/json"),
        new ("Authorization", "Basic"),
    ];

    [ObservableProperty] private Dictionary<string, string>? _responseHeaders;

    public HttpMethod[] HttpMethods { get; } =
        [HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete, HttpMethod.Options];

    [RelayCommand(CanExecute = nameof(CanMakeRequest))]
    private async Task MakeRequest()
    {
        using HttpClient client = new();
        HttpRequestMessage request = new(SelectedMethod, Url);
        Stopwatch stopwatch = new();

        if (!string.IsNullOrEmpty(Body))
        {
            request.Content = new StringContent(Body);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        stopwatch.Start();
        HttpResponseMessage response = await client.SendAsync(request);
        stopwatch.Stop();

        await UpdateResponseData(response, stopwatch.Elapsed);
    }

    private bool CanMakeRequest() =>
        Uri.TryCreate(Url, UriKind.Absolute, out Uri? _);

    [RelayCommand]
    private void AddHeader() =>
        RequestHeaders.Add(new RequestHeader(string.Empty, string.Empty));

    [RelayCommand]
    private void RemoveHeader(object? parameter)
    {
        if (parameter is not Guid id)
            return;
        RequestHeaders.Remove(RequestHeaders.First(f => f.Id == id));
    }

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

public class RequestHeader(string key, string value) : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}