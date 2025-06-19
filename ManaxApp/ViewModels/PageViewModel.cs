using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxApp.ViewModels;

public abstract class PageViewModel:ObservableObject
{
    public EventHandler<PageViewModel>? PageChangedRequested;
}