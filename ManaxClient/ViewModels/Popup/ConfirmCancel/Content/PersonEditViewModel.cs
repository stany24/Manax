using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Role;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public partial class PersonEditViewModel : ConfirmCancelContentViewModel
{
    private readonly long _personId;
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _pseudonym;
    [ObservableProperty] private RoleDto _role;
    public ObservableCollection<RoleDto> Roles { get; }

    public PersonEditViewModel(long personId, PersonUpdateDto person)
    {
        _personId = personId;
        _firstName = person.FirstName;
        _lastName = person.LastName;
        _pseudonym = person.Pseudonym;
        _role = person.Role;
        CanConfirm = true;

        Roles = [];

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
            Role = Role
        };
    }

    public PersonCreateDto GetCreateResult()
    {
        return new PersonCreateDto
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Pseudonym = string.IsNullOrEmpty(Pseudonym) ? string.Empty : Pseudonym.Trim(),
            Role = Role
        };
    }
}

