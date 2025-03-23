using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

    [ObservableProperty] private HttpStatusCode? _statusCode;

    [ObservableProperty] private string? _responseContent;

    [ObservableProperty] private ObservableCollection<HttpHeader> _requestHeaders =
    [
        new ("Accept", "application/json"),
        new ("Content-Type", "application/json")
    ];

    [ObservableProperty] private ObservableCollection<HttpHeader> _responseHeaders = new();

    public HttpMethod[] HttpMethods { get; } =
        [HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete, HttpMethod.Options];

    private void ClearResponseHeaders()
    {
        ResponseHeaders.Clear();
    }

    [RelayCommand(CanExecute = nameof(CanMakeRequest))]
    private async Task MakeRequest()
    {
        ClearResponseHeaders();
        using HttpClient client = new();
        HttpRequestMessage request = new(SelectedMethod, Url);
        
        if (RequestHeaders is { Count: > 0 })
        {
            foreach (HttpHeader header in RequestHeaders)
            {
                if (header.Key != "Content-Type")
                    request.Headers.Add(header.Key, header.Value);
            }
        }

        if (!string.IsNullOrEmpty(Body))
        {
            request.Content = new StringContent(Body);
            foreach (HttpHeader header in RequestHeaders)
            {
                if (header.Key == "Content-Type")
                    request.Content.Headers.Add(header.Key, header.Value);
            }
        }
        
        Stopwatch stopwatch = new();
        stopwatch.Start();
        HttpResponseMessage response = await client.SendAsync(request);
        stopwatch.Stop();

        await UpdateResponseData(response, stopwatch.Elapsed);
        SetResponseHeaders(response);
    }

    private void SetResponseHeaders(HttpResponseMessage response)
    {
        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers)
        {
            ResponseHeaders.Add(new (header.Key, string.Join(", ", header.Value)));
        }
        
        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
        {
            ResponseHeaders.Add(new (header.Key, string.Join(", ", header.Value)));
        }
    }

    private bool CanMakeRequest() =>
        Uri.TryCreate(Url, UriKind.Absolute, out Uri? _);

    [RelayCommand]
    private void AddHeader() =>
        RequestHeaders.Add(new HttpHeader(string.Empty, string.Empty));

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

        StatusCode = response.StatusCode;
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

public class HttpHeader(string key, string value) : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}