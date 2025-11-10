using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Person : ObservableObject
{
    [ObservableProperty] private long _id ;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _pseudonym = string.Empty;
    [ObservableProperty] private RoleDto _role;

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
        Role = dto.Role;
    }

    private void OnPersonUpdated(PersonDto dto)
    {
        if (Id != dto.Id) return;
        FromDto(dto);
    }
}