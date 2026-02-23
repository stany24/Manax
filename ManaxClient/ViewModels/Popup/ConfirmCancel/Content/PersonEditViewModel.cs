using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxLibrary.DTO.Person;
using Role = ManaxClient.Models.Server.Data.Role;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class PersonEditViewModel : ConfirmCancelContentViewModel
{
    private readonly long _personId;
    private readonly ReadOnlyObservableCollection<Role> _roles;
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _pseudonym;
    [ObservableProperty] private Role? _role;

    public PersonEditViewModel(long personId, PersonUpdateDto person)
    {
        _personId = personId;
        _firstName = person.FirstName;
        _lastName = person.LastName;
        _pseudonym = person.Pseudonym;
        CanConfirm = true;

        SortExpressionComparer<Role> roleComparer = SortExpressionComparer<Role>
            .Ascending(r => r.Name);
        MainWindowViewModel.Instance.RoleSource.Roles.Connect()
            .SortAndBind(out _roles, roleComparer)
            .Subscribe();

        _role = _roles.FirstOrDefault(r => r.Id == person.RoleId);

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(FirstName) or nameof(LastName))
                CanConfirm = !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);
        };
    }

    public ReadOnlyObservableCollection<Role> Roles => _roles;

    public long GetPersonId()
    {
        return _personId;
    }

    public PersonUpdateDto GetResult()
    {
        return new PersonUpdateDto
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Pseudonym = string.IsNullOrEmpty(Pseudonym) ? string.Empty : Pseudonym.Trim(),
            RoleId = Role?.Id ?? 0
        };
    }

    public PersonCreateDto GetCreateResult()
    {
        return new PersonCreateDto
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Pseudonym = string.IsNullOrEmpty(Pseudonym) ? string.Empty : Pseudonym.Trim(),
            RoleId = Role?.Id ?? 0
        };
    }
}