using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace Lance.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(MakeRequestCommand))]
    private string _url = string.Empty;

    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(MakeRequestCommand))]
    private int _selectedMethodIndex;

    [ObservableProperty] private string? _body;

    [ObservableProperty] private bool _requestCompleted;

    [ObservableProperty] private string? _formattedRequestTime;

    [ObservableProperty] private string? _formattedStatusCode;

    [ObservableProperty] private string? _responseContent;
    
    private readonly HttpMethod[] _indexToHttpMethodArray =
        [HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete, HttpMethod.Options];

    [RelayCommand(CanExecute = nameof(CanMakeRequest))]
    private async Task MakeRequest()
    {
        using HttpClient client = new();
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response =
            await client.SendAsync(new HttpRequestMessage(_indexToHttpMethodArray[SelectedMethodIndex], Url));
        stopwatch.Stop();

        await UpdateResponseData(response, stopwatch.Elapsed);
    }

    private bool CanMakeRequest() =>
        !string.IsNullOrEmpty(Url) && !string.IsNullOrWhiteSpace(Url) && SelectedMethodIndex > -1;

    private async Task UpdateResponseData(HttpResponseMessage response, TimeSpan requestDuration)
    {
        Task<string> stringResponseContent = response.Content.ReadAsStringAsync();
        
        FormattedRequestTime = $"{Math.Round(requestDuration.TotalMilliseconds)} ms";

        FormattedStatusCode = $"{(int)response.StatusCode} - {response.ReasonPhrase}";

        if (response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string>? contentTypes))
        {
            if (contentTypes.Any(a => a.Contains("application/json")))
            {
                object? obj = JsonSerializer.Deserialize<object?>(await stringResponseContent);

                if (obj is not null)
                    ResponseContent = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        else
        {
            ResponseContent = await stringResponseContent;
        }

        RequestCompleted = true;
    }
}