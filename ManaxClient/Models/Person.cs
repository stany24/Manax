using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Person : ObservableObject
{
    [ObservableProperty] private long _id ;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _pseudonym = string.Empty;
    [ObservableProperty] private Role _role = null!;
    public string FullName => $"{FirstName} ({Pseudonym}) {LastName}";

    public Person(PersonDto dto)
    {
        ServerNotification.OnPersonUpdated += OnPersonUpdated;
        FromDto(dto);
    }

    ~Person()
    {
        ServerNotification.OnPersonUpdated -= OnPersonUpdated;
    }

    private void FromDto(PersonDto dto)
    {
        Id = dto.Id;
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        Pseudonym = dto.Pseudonym;
        Role = RoleSource.Roles.Items.First(r => r.Id == dto.Role.Id);
    }

    private void OnPersonUpdated(PersonDto dto)
    {
        if (Id != dto.Id) return;
        FromDto(dto);
    }
    
    public PersonDto ToPersonDto()
    {
        return new PersonDto
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            Pseudonym = Pseudonym,
            Role = Role.ToRoleDto()
        };
    }
}