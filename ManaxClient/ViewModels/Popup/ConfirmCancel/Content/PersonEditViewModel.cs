using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Person;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class PersonEditViewModel : ConfirmCancelContentViewModel
{
    private readonly long _personId;
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _pseudonym;
    [ObservableProperty] private Models.Role? _role;
    
    public ReadOnlyObservableCollection<Models.Role> Roles => _roles;
    private readonly ReadOnlyObservableCollection<Models.Role> _roles;

    public PersonEditViewModel(long personId, PersonUpdateDto person)
    {
        _personId = personId;
        _firstName = person.FirstName;
        _lastName = person.LastName;
        _pseudonym = person.Pseudonym;
        CanConfirm = true;

        SortExpressionComparer<Models.Role> roleComparer = SortExpressionComparer<Models.Role>
            .Ascending(r => r.Name);
        RoleSource.Roles.Connect()
            .SortAndBind(out _roles, roleComparer)
            .Subscribe();

        // Find the matching role from the source
        _role = _roles.FirstOrDefault(r => r.Id == person.RoleId);

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(FirstName) || args.PropertyName == nameof(LastName))
            {
                CanConfirm = !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);
            }
        };
    }

    public long GetPersonId() => _personId;

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

