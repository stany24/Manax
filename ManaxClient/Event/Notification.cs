using System;

namespace ManaxClient.Event;

public class Notification(string localizationKey)
{
    public string LocalizationKey { get; init; } = localizationKey;
    public object[] Args { get; init; } = [];
    public EventHandler? RemoveRequested;

    public Notification(string localizationKey, object[] args): this(localizationKey)
    {
        Args = args;
    }

    public void Remove()
    {
        RemoveRequested?.Invoke(this, EventArgs.Empty);
    }
}