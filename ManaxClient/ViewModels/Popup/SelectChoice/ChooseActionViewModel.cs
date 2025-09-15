using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Popup.SelectChoice;

public partial class ChooseActionViewModel(List<string> options) : PopupViewModel
{
    [ObservableProperty] private string _selectedAction = string.Empty;
    public ObservableCollection<string> Options { get; } = new(options);

    public void SelectAction(string? action)
    {
        if (action == null) return;
        SelectedAction = action;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public string GetResult()
    {
        return SelectedAction;
    }

    public override bool CloseAccepted()
    {
        return true;
    }
}