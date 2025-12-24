using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ManaxClient.Event;

public class PermissionChangedMessage(KeyValuePair<string,bool> page) : ValueChangedMessage<KeyValuePair<string,bool>>(page);