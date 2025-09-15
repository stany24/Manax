using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Rank;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class RankEditViewModel : ConfirmCancelContentViewModel
{
    private readonly RankDto _originalRank;
    [ObservableProperty] private string _name;
    [ObservableProperty] private double _value;

    public RankEditViewModel(RankDto rank)
    {
        _originalRank = rank;
        _name = rank.Name;
        _value = rank.Value;
        CanConfirm = true;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Name)) CanConfirm = !string.IsNullOrWhiteSpace(Name);
        };
    }

    public RankDto GetResult()
    {
        return new RankDto
        {
            Id = _originalRank.Id,
            Name = Name.Trim(),
            Value = (int)Value
        };
    }
}