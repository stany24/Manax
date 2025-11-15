using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Localization;

public partial class Language: ObservableObject
{
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string _displayName = string.Empty;
}