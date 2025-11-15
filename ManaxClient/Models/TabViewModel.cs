using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models;

public class TabViewModel : ObservableObject
{
    public EventHandler<TabViewModel?>? NextRequested { get; set; }
}