using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public static class PersonSource
{
    public static readonly SourceCache<Data.Person, long> Persons = new(x => x.Id);
    private static bool _loaded;
    private static readonly Lock LoadLock = new();
    private static readonly Lock PersonsLock = new();

    static PersonSource()
    {
        ServerNotification.OnPersonCreated += OnPersonCreated;
        ServerNotification.OnPersonDeleted += OnPersonDeleted;
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

    private static void OnPersonDeleted(long id)
    {
        lock (PersonsLock)
        {
            Persons.RemoveKey(id);
        }
    }

    private static void OnPersonCreated(PersonDto dto)
    {
        lock (PersonsLock)
        {
            Persons.AddOrUpdate(new Data.Person(dto));
        }
    }

    public static void LoadPersons()
    {
        Task.Run(() =>
        {
            lock (LoadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<PersonDto>> personsResponse = ManaxApiPersonClient.GetPersonsAsync().Result;
                    if (personsResponse.Failed)
                    {
                        Logger.LogFailure(personsResponse.Error);
                        ErrorEmitted?.Invoke(null, personsResponse.Error);
                        return;
                    }

                    lock (PersonsLock)
                    {
                        Persons.Edit(updater =>
                        {
                            updater.Clear();
                            List<Data.Person> persons = personsResponse.GetValue().Select(dto => new Data.Person(dto)).ToList();
                            updater.AddOrUpdate(persons);
                        });
                    }

                    _loaded = true;
                }
                catch (Exception e)
                {
                    const string error = "Failed to load ranks from server";
                    Logger.LogError(error, e);
                    ErrorEmitted?.Invoke(null, error);
                }
            }
        });
    }
}