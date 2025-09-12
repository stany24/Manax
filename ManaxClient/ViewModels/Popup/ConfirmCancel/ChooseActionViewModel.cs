using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel;

public partial class ChooseActionViewModel : ConfirmCancelContentViewModel
{
    public ObservableCollection<string> Options { get; } = [];
    [ObservableProperty] private string _selectedAction = string.Empty;

    public ICommand SelectActionCommand { get; }

    public ChooseActionViewModel(List<string> options)
    {
        SelectActionCommand = new RelayCommand<string>(SelectAction);
        
        foreach (string option in options)
            Options.Add(option);
        
        if (Options.Count > 0)
        {
            CanConfirm = true;
        }
    }

    public void SelectAction(string? action)
    {
        if (action != null)
        {
            SelectedAction = action;
            CanConfirm = true;
        }
    }

    public string GetResult()
    {
        return SelectedAction;
    }
}
