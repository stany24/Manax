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
using ManaxLibrary.DTO.Role;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class RoleSource
{
    public readonly SourceCache<Role, long> Roles = new(x => x.Id);
    private bool _loaded;
    private readonly Lock _loadLock = new();
    private readonly Lock _rolesLock = new();

    public RoleSource()
    {
        NotificationReceiver.OnRoleCreated += OnRoleCreated;
        NotificationReceiver.OnRoleDeleted += OnRoleDeleted;
    }

    private void OnRoleDeleted(long id)
    {
        lock (_rolesLock)
        {
            Roles.RemoveKey(id);
        }
    }

    private void OnRoleCreated(RoleDto dto)
    {
        lock (_rolesLock)
        {
            Roles.AddOrUpdate(new Role(dto));
        }
    }

    public void LoadRoles()
    {
        Task.Run(() =>
        {
            lock (_loadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<RoleDto>> ranksResponse = ManaxApiRoleClient.GetRolesAsync().Result;
                    if (ranksResponse.Failed)
                    {
                        Logger.LogFailure(ranksResponse.Error);
                        WeakReferenceMessenger.Default.Send(new NotificationMessage(ranksResponse.Error));
                        return;
                    }

                    lock (_rolesLock)
                    {
                        Roles.Edit(updater =>
                        {
                            updater.Clear();
                            List<Role> ranks = ranksResponse.GetValue().Select(dto => new Role(dto)).ToList();
                            updater.AddOrUpdate(ranks);
                        });
                    }

                    _loaded = true;
                }
                catch (Exception e)
                {
                    const string error = "Failed to load ranks from server";
                    Logger.LogError(error, e);
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                }
            }
        });
    }
}