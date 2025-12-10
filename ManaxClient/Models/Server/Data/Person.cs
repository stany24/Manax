using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Data;

public partial class Person : ObservableObject
{
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _pseudonym = string.Empty;
    [ObservableProperty] private Role _role = null!;

    public Person(PersonDto dto)
    {
        NotificationReceiver.OnPersonUpdated += OnPersonUpdated;
        FromDto(dto);
    }

    public string FullName => $"{FirstName} ({Pseudonym}) {LastName}";

    ~Person()
    {
        NotificationReceiver.OnPersonUpdated -= OnPersonUpdated;
    }

    private void FromDto(PersonDto dto)
    {
        Id = dto.Id;
        FirstName = dto.FirstName;
        LastName = dto.LastName;
        Pseudonym = dto.Pseudonym;
        Role = MainWindowViewModel.Instance.RoleSource.Roles.Items.First(r => r.Id == dto.RoleId);
    }

    private void OnPersonUpdated(PersonDto dto)
    {
        if (Id != dto.Id) return;
        FromDto(dto);
    }
}