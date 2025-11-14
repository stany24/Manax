using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Person;

public class PersonPageViewModel : PageViewModel
{
    public ReadOnlyObservableCollection<Models.Person> Persons => _persons;
    private readonly ReadOnlyObservableCollection<Models.Person> _persons;
    public ReadOnlyObservableCollection<Models.Role> Roles => _roles;
    private readonly ReadOnlyObservableCollection<Models.Role> _roles;

    public PersonPageViewModel()
    {
        SortExpressionComparer<Models.Person> comparer = SortExpressionComparer<Models.Person>
            .Ascending(p => p.LastName)
            .ThenByAscending(p => p.FirstName);
        PersonSource.Persons.Connect()
            .SortAndBind(out _persons, comparer)
            .Subscribe();

        SortExpressionComparer<Models.Role> roleComparer = SortExpressionComparer<Models.Role>
            .Ascending(r => r.Name);
        RoleSource.Roles.Connect()
            .SortAndBind(out _roles, roleComparer)
            .Subscribe();
    }

    public void UpdatePerson(Models.Person person)
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
                    InfoEmitted?.Invoke(this, updatePersonAsync.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to update person on server");
                Logger.LogError("Failed to update person on server", e);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }

    public void DeletePerson(Models.Person person)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<bool> deletePersonResponse = await ManaxApiPersonClient.DeletePersonAsync(person.Id);
                if (deletePersonResponse.Failed) InfoEmitted?.Invoke(this, deletePersonResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to delete person on server");
                Logger.LogError("Failed to delete person on server", e);
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
                    InfoEmitted?.Invoke(this, personResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to create person on server");
                Logger.LogError("Failed to create person on server", e);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }

    public void UpdateRole(Models.Role role)
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
                    InfoEmitted?.Invoke(this, updateRoleAsync.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to update role on server");
                Logger.LogError("Failed to update role on server", e);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }

    public void DeleteRole(Models.Role role)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<bool> deleteRoleResponse = await ManaxApiRoleClient.DeleteRoleAsync(role.Id);
                if (deleteRoleResponse.Failed) InfoEmitted?.Invoke(this, deleteRoleResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to delete role on server");
                Logger.LogError("Failed to delete role on server", e);
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
                    InfoEmitted?.Invoke(this, roleResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to create role on server");
                Logger.LogError("Failed to create role on server", e);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}