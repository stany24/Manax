using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models;

public partial class TabViewModel:ObservableObject
{
    public EventHandler? NextRequested;
}