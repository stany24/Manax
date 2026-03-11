using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Models.Server.Data;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class PersonSource
{
    private readonly Lock _loadLock = new();
    private readonly Lock _personsLock = new();
    public readonly SourceCache<Person, long> Persons = new(x => x.Id);
    private bool _loaded;

    public PersonSource()
    {
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, _) => LoadPersons());
        NotificationReceiver.OnPersonCreated += OnPersonCreated;
        NotificationReceiver.OnPersonDeleted += OnPersonDeleted;
    }

    private void OnPersonDeleted(long id)
    {
        lock (_personsLock)
        {
            Persons.RemoveKey(id);
        }
    }

    private void OnPersonCreated(PersonDto dto)
    {
        lock (_personsLock)
        {
            Persons.AddOrUpdate(new Person(dto));
        }
    }

    private void LoadPersons()
    {
        Task.Run(() =>
        {
            lock (_loadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<PersonDto>> personsResponse = ManaxApiPersonClient.GetPersonsAsync().Result;
                    if (personsResponse.Failed)
                    {
                        Logger.LogFailure(personsResponse.Error);
                        WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(personsResponse.Error)));
                        return;
                    }

                    lock (_personsLock)
                    {
                        Persons.Edit(updater =>
                        {
                            updater.Clear();
                            List<Person> persons = personsResponse.GetValue().Select(dto => new Person(dto)).ToList();
                            updater.AddOrUpdate(persons);
                        });
                    }

                    _loaded = true;
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to load ranks from server", e);
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification("Person.Load.Failed")));
                }
            }
        });
    }
}