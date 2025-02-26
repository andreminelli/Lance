using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lance.Models;

public class HttpHeaderModel(string key, string value) : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
}