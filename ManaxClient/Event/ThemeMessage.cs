using CommunityToolkit.Mvvm.Messaging.Messages;
using ManaxClient.Models.Theme;

namespace ManaxClient.Event;

public class ThemeMessage(ThemeSettingsData theme) : ValueChangedMessage<ThemeSettingsData>(theme);