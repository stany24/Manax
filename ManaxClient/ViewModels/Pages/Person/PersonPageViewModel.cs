using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Event;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.Logging;
using Role = ManaxClient.Models.Server.Data.Role;

namespace ManaxClient.ViewModels.Pages.Person;

public class PersonPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Server.Data.Person> _persons;
    private readonly ReadOnlyObservableCollection<Role> _roles;

    public PersonPageViewModel()
    {
        SortExpressionComparer<Models.Server.Data.Person> comparer = SortExpressionComparer<Models.Server.Data.Person>
            .Ascending(p => p.LastName)
            .ThenByAscending(p => p.FirstName);
        MainWindowViewModel.Instance.PersonSource.Persons.Connect()
            .SortAndBind(out _persons, comparer)
            .Subscribe();

        SortExpressionComparer<Role> roleComparer = SortExpressionComparer<Role>
            .Ascending(r => r.Name);
        MainWindowViewModel.Instance.RoleSource.Roles.Connect()
            .SortAndBind(out _roles, roleComparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Models.Server.Data.Person> Persons => _persons;
    public ReadOnlyObservableCollection<Role> Roles => _roles;

    public void UpdatePerson(Models.Server.Data.Person person)
    {
        PersonUpdateDto update = new()
        {
            FirstName = person.FirstName,
            LastName = person.LastName,
            Pseudonym = person.Pseudonym,
            RoleId = person.Role.Id
        };
        PersonEditViewModel content = new(person.Id, update);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                PersonUpdateDto result = content.GetResult();
                long personId = content.GetPersonId();
                Optional<bool> updatePersonAsync = await ManaxApiPersonClient.UpdatePersonAsync(personId, result);
                if (updatePersonAsync.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(updatePersonAsync.Error));
            }
            catch (Exception e)
            {
                const string error = "Failed to update person on server";
                WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                Logger.LogError(error, e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }

    public void DeletePerson(Models.Server.Data.Person person)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<bool> deletePersonResponse = await ManaxApiPersonClient.DeletePersonAsync(person.Id);
                if (deletePersonResponse.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(deletePersonResponse.Error));
            }
            catch (Exception e)
            {
                const string error = "Failed to delete person on server";
                WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                Logger.LogError(error, e);
            }
        });
    }

    public void CreatePerson()
    {
        PersonUpdateDto initialData = new()
        {
            FirstName = "New",
            LastName = "Person",
            Pseudonym = string.Empty,
            RoleId = 0
        };
        PersonEditViewModel content = new(0, initialData);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                PersonCreateDto result = content.GetCreateResult();
                Optional<bool> personResponse = await ManaxApiPersonClient.CreatePersonAsync(result);

                if (personResponse.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(personResponse.Error));
            }
            catch (Exception e)
            {
                const string error = "Failed to create person on server";
                WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                Logger.LogError(error, e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }

    public void UpdateRole(Role role)
    {
        RoleUpdateDto update = new()
        {
            Id = role.Id,
            Name = role.Name
        };
        RoleEditViewModel content = new(update);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                RoleUpdateDto result = content.GetResult();
                Optional<bool> updateRoleAsync = await ManaxApiRoleClient.UpdateRoleAsync(result);
                if (updateRoleAsync.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(updateRoleAsync.Error));
            }
            catch (Exception e)
            {
                const string error = "Failed to update role on server";
                WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                Logger.LogError(error, e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }

    public void DeleteRole(Role role)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<bool> deleteRoleResponse = await ManaxApiRoleClient.DeleteRoleAsync(role.Id);
                if (deleteRoleResponse.Failed) 
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(deleteRoleResponse.Error));
            }
            catch (Exception e)
            {
                const string error = "Failed to delete role on server";
                WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                Logger.LogError(error, e);
            }
        });
    }

    public void CreateRole()
    {
        RoleEditViewModel content = new(new RoleUpdateDto { Name = "New Role" });
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                RoleUpdateDto result = content.GetResult();
                Optional<bool> roleResponse = await ManaxApiRoleClient.CreateRoleAsync(new RoleCreateDto
                    { Name = result.Name });

                if (roleResponse.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(roleResponse.Error));
            }
            catch (Exception e)
            {
                const string error = "Failed to create role on server";
                WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                Logger.LogError(error, e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }
}