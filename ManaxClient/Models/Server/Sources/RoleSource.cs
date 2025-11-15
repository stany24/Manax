using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public static class RoleSource
{
    public static readonly SourceCache<Data.Role, long> Roles = new(x => x.Id);
    private static bool _loaded;
    private static readonly Lock LoadLock = new();
    private static readonly Lock RolesLock = new();

    static RoleSource()
    {
        ServerNotification.OnRoleCreated += OnRoleCreated;
        ServerNotification.OnRoleDeleted += OnRoleDeleted;
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

    private static void OnRoleDeleted(long id)
    {
        lock (RolesLock)
        {
            Roles.RemoveKey(id);
        }
    }

    private static void OnRoleCreated(RoleDto dto)
    {
        lock (RolesLock)
        {
            Roles.AddOrUpdate(new Data.Role(dto));
        }
    }

    public static void LoadRoles()
    {
        Task.Run(() =>
        {
            lock (LoadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<RoleDto>> ranksResponse = ManaxApiRoleClient.GetRolesAsync().Result;
                    if (ranksResponse.Failed)
                    {
                        Logger.LogFailure(ranksResponse.Error);
                        ErrorEmitted?.Invoke(null, ranksResponse.Error);
                        return;
                    }

                    lock (RolesLock)
                    {
                        Roles.Edit(updater =>
                        {
                            updater.Clear();
                            List<Data.Role> ranks = ranksResponse.GetValue().Select(dto => new Data.Role(dto)).ToList();
                            updater.AddOrUpdate(ranks);
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