using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace Lance.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _url = string.Empty;

    [ObservableProperty] private int _selectedMethodIndex;

    [ObservableProperty] private string? _body;

    [ObservableProperty] private bool _requestCompleted;

    [ObservableProperty] private string? _formattedRequestTime;

    [ObservableProperty] private string? _formattedStatusCode;

    [ObservableProperty] private string? _responseContent;

    private readonly HttpMethod[] _indexToHttpMethodArray =
        [HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete, HttpMethod.Options];

    [RelayCommand]
    private async Task MakeRequest()
    {
        using HttpClient client = new();

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Stopwatch stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response =
            await client.SendAsync(new HttpRequestMessage(_indexToHttpMethodArray[SelectedMethodIndex], Url));
        stopwatch.Stop();

        await UpdateResponseData(response, stopwatch.Elapsed);
    }

    private async Task UpdateResponseData(HttpResponseMessage response, TimeSpan requestDuration)
    {
        Task<string> stringResponseContent = response.Content.ReadAsStringAsync();

        FormattedRequestTime = $"{requestDuration.TotalMilliseconds} ms";

        FormattedStatusCode = $"{(int)response.StatusCode} - {response.ReasonPhrase}";

        object? obj = JsonSerializer.Deserialize<object?>(await stringResponseContent);

        if (obj is not null)
            ResponseContent = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });

        RequestCompleted = true;
    }
}