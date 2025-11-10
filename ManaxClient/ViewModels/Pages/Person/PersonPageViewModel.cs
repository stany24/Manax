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
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Person;

public class PersonPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Person> _persons;

    public PersonPageViewModel()
    {
        SortExpressionComparer<Models.Person> comparer = SortExpressionComparer<Models.Person>
            .Ascending(p => p.LastName)
            .ThenByAscending(p => p.FirstName);
        PersonSource.Persons.Connect()
            .SortAndBind(out _persons, comparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Models.Person> Persons => _persons;

    public void UpdatePerson(Models.Person person)
    {
        PersonUpdateDto update = new()
        {
            FirstName = person.FirstName,
            LastName = person.LastName,
            Pseudonym = person.Pseudonym,
            Role = person.Role
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
            Role = new RoleDto { Id = 0, Name = "Unknown" }
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
}