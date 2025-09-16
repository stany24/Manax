using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Rank;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class RankEditViewModel : ConfirmCancelContentViewModel
{
    private readonly RankUpdateDto _originalRank;
    [ObservableProperty] private string _name;
    [ObservableProperty] private double _value;

    public RankEditViewModel(RankUpdateDto rank)
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

    public RankUpdateDto GetResult()
    {
        return new RankUpdateDto
        {
            Id = _originalRank.Id,
            Name = Name.Trim(),
            Value = (int)Value
        };
    }
}