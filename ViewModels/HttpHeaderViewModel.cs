using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lance.ViewModels;

public class HttpHeaderViewModel(string key, string value) : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}